using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using Android.Content;
using Android.Hardware.Usb;
using Hoho.Android.UsbSerial.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Util;
using Android.Views;

[assembly: UsesFeature("android.hardware.usb.host")]

namespace AppYMFC32panel
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
    public class MainActivity : AppCompatActivity
    {

        EditText emailEditText, passwordEditText;
        Button signInButton, registerButton, serialButton;

        UsbManager usbManager;
        UsbSerialPortAdapter adapter;
        BroadcastReceiver detachedReceiver;
        BroadcastReceiver attachedReceiver;
        UsbSerialPort selectedPort;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            usbManager = GetSystemService(Context.UsbService) as UsbManager;
            emailEditText = FindViewById<EditText>(Resource.Id.emailEditText);
            passwordEditText = FindViewById<EditText>(Resource.Id.passwordEditText);
            signInButton = FindViewById<Button>(Resource.Id.signinButton);
            registerButton = FindViewById<Button>(Resource.Id.registerButton);
            serialButton = FindViewById<Button>(Resource.Id.serialButton);

            signInButton.Click += SignInButton_Click;
            registerButton.Click += RegisterButton_Click;
            serialButton.Click += SerialButton_Click;
        }

        private async void SerialButton_Click(object sender, EventArgs e)
        {
            /////////////SerialActivity///////////
            selectedPort = adapter.GetItem(0);
            var permissionGranted = await usbManager.RequestPermissionAsync(selectedPort.Driver.Device, this);
            if (permissionGranted)
            {
                var newIntent = new Intent(this, typeof(NavigationSerialActivity));
                newIntent.PutExtra(NavigationSerialActivity.EXTRA_TAG, new UsbSerialPortInfo(selectedPort));

                StartActivity(newIntent);
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(RegisterActivity));
            intent.PutExtra("email", emailEditText.Text);
            StartActivity(intent);
        }

        private void SignInButton_Click(object sender, EventArgs e)
        {
            /////////////blutoothActivity///////////
            var newIntent = new Intent(this, typeof(NavigationBluetoothActivity));
            StartActivity(newIntent);
        }
     
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override async void OnResume()
        {
            base.OnResume();

            adapter = new UsbSerialPortAdapter(this);
     

            await PopulateListAsync();

            //register the broadcast receivers
            detachedReceiver = new UsbDeviceDetachedReceiver(this);
            RegisterReceiver(detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
            attachedReceiver = new UsbDeviceAttachedReceiver(this);
            RegisterReceiver(attachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));
        }
        protected override void OnPause()
        {
            base.OnPause();

            // unregister the broadcast receivers
            var temp = detachedReceiver; // copy reference for thread safety
            if (temp != null)
                UnregisterReceiver(temp);
        }
        internal static Task<IList<IUsbSerialDriver>> FindAllDriversAsync(UsbManager usbManager)
        {
            // using the default probe table
            var table = UsbSerialProber.DefaultProbeTable;
            var prober = new UsbSerialProber(table);
            return prober.FindAllDriversAsync(usbManager);
        }

        async Task PopulateListAsync()
        {
            var drivers = await FindAllDriversAsync(usbManager);

            adapter.Clear();
            foreach (var driver in drivers)
            {
                var ports = driver.Ports;
                foreach (var port in ports)
                    adapter.Add(port);
            }

            adapter.NotifyDataSetChanged();
        }


        #region UsbSerialPortAdapter implementation

        class UsbSerialPortAdapter : ArrayAdapter<UsbSerialPort>
        {
            public UsbSerialPortAdapter(Context context)
                : base(context, global::Android.Resource.Layout.SimpleExpandableListItem2)
            {
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var row = convertView;
                if (row == null)
                {
                    var inflater = Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                    row = inflater.Inflate(global::Android.Resource.Layout.SimpleListItem2, null);
                }

                var port = GetItem(position);
                var driver = port.GetDriver();
                var device = driver.GetDevice();

                var title = string.Format("Vendor {0} Product {1}",
                    HexDump.ToHexString((short)device.VendorId),
                    HexDump.ToHexString((short)device.ProductId));
                row.FindViewById<TextView>(global::Android.Resource.Id.Text1).Text = title;

                var subtitle = device.Class.SimpleName;
                row.FindViewById<TextView>(global::Android.Resource.Id.Text2).Text = subtitle;

                return row;
            }
        }

        #endregion

        #region UsbDeviceDetachedReceiver implementation

        class UsbDeviceDetachedReceiver
            : BroadcastReceiver
        {
            readonly MainActivity activity;

            public UsbDeviceDetachedReceiver(MainActivity activity)
            {
                this.activity = activity;
            }

            public async override void OnReceive(Context context, Intent intent)
            {

                await activity.PopulateListAsync();
            }
        }

        #endregion

        #region UsbDeviceAttachedReceiver implementation

        class UsbDeviceAttachedReceiver
            : BroadcastReceiver
        {
            readonly MainActivity activity;

            public UsbDeviceAttachedReceiver(MainActivity activity)
            {
                this.activity = activity;
            }

            public override async void OnReceive(Context context, Intent intent)
            {
                await activity.PopulateListAsync();
            }
        }

        #endregion
    }
}