using System;
using System.Linq;
using Android.Bluetooth;
using Android.App;
using Android.OS;
using Android.Widget;
using Java.Util;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Views;
using Android.Support.Design.Widget;
using Android.Animation;
using System.Drawing;

namespace AppYMFC32panel
{
    [Activity(Label = "NavigationBluetoothActivity")]
    public class NavigationBluetoothActivity : MainActivity, IOnMapReadyCallback
    {

        ImageButton connectButton;
        EditText bluetoothData1;
        EditText bluetoothData2;
        EditText bluetoothData3;
        EditText bluetoothData4;
        EditText bluetoothData5;
        EditText bluetoothData6;
        EditText bluetoothData7;
        EditText bluetoothData8;

        ProgressBar progressBar1;
        MarkerOptions marker = new MarkerOptions();
        MarkerOptions home = new MarkerOptions();
        MarkerOptions JMCdrone = new MarkerOptions();
        PolygonOptions wPpolygon = new PolygonOptions();
        Marker jmcMarker;
        MapFragment mapFragment;
        private static bool isFabOpen;
        private FloatingActionButton floatingButton1;
        private FloatingActionButton floatingButton2;
        private FloatingActionButton floatingButton3;
        private FloatingActionButton floatingButton4;

        System.Random myRandowInt = new System.Random();
        private BluetoothSocket _socket;
        private byte[] buffer = { 1 };

        public bool create_waypoint_list;
        public int time_counter, receive_buffer_counter, receive_start_detect;
        public byte[] receive_buffer = new byte[50];
        public byte[] send_buffer = new byte[20];
        public byte receive_byte_previous, home_gps_set;

        public byte check_byte, temp_byte, start;
        public byte first_receive, received_data, webbrouwser_active;

        public long milliseconds, last_receive;
        public double ground_distance, los_distance;

        public int zoom = 17;
        public int minSats = 2;

        public LatLng latLongJMCdrone;
        public LatLng homeLatLong;

        public LatLng[] wayPointArray = new LatLng[10];

        public float fixCameraHomePoint = 1,
            fixCameraHomePointDivisor = 2;

        public short temperature;
        public int error, flight_mode, roll_angle, pitch_angle;
        public int altitude_meters,
            max_altitude_meters,
            takeoff_throttle,
            actual_compass_heading,
            heading_lock,
            number_used_sats,
            fix_type,
            l_lat_gps,
            l_lon_gps,
            home_lat_gps,
            home_lon_gps;

        public int addWayPointCounter, send_telemetry_data_counter, waypoint_send_step = 0;
        public int flight_timer_seconds;

        public int new_telemetry_data_to_send;
        public int battery_bar_level;


        public float battery_voltage,
            adjustable_setting_1,
            adjustable_setting_2,
            adjustable_setting_3;

        public string location_address;

        int floatingButton2_clicked_counter = 0;
        bool floatingButton2_clicked = true;
        bool floatingButton3_clicked = false;
        bool floatingButton4_clicked = false;
        bool boolDragEnd = false;
        bool fixWayPoint = true;
        int flyWayPointCounter = 0;
        int startMission;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.navigationPage);

            bluetoothData1 = FindViewById<EditText>(Resource.Id.bluetoothDataEditText1);
            bluetoothData2 = FindViewById<EditText>(Resource.Id.bluetoothDataEditText2);
            bluetoothData3 = FindViewById<EditText>(Resource.Id.bluetoothDataEditText3);
            bluetoothData4 = FindViewById<EditText>(Resource.Id.bluetoothDataEditText4);
            bluetoothData5 = FindViewById<EditText>(Resource.Id.bluetoothDataEditText5);
            bluetoothData6 = FindViewById<EditText>(Resource.Id.bluetoothDataEditText6);
            bluetoothData7 = FindViewById<EditText>(Resource.Id.bluetoothDataEditText7);
            bluetoothData8 = FindViewById<EditText>(Resource.Id.bluetoothDataEditText8);
            progressBar1 = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            connectButton = FindViewById<ImageButton>(Resource.Id.connectButton);
            mapFragment = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.mapFragment);
            floatingButton1 = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButton1);
            floatingButton2 = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButton2);
            floatingButton3 = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButton3);
            floatingButton4 = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButton4);

            connectButton.Click += ConnectButton_Click;

            floatingButton1.Click += (o, e) =>
            {
                if (!isFabOpen)
                    ShowFabMenu();
                else
                    CloseFabMenu();
            };

            floatingButton2.Click += (o, e) =>
            {
                floatingButton2_clicked = true;
                floatingButton2_clicked_counter++;
                Toast.MakeText(this, floatingButton2_clicked_counter.ToString(), ToastLength.Short).Show();
            };

            floatingButton3.Click += (o, e) =>
            {
                floatingButton3_clicked = true;
                fixWayPoint = true;
                flyWayPointCounter = 0;
                Toast.MakeText(this, "Airballon!", ToastLength.Short).Show();
            };
            floatingButton4.Click += (o, e) =>
            {
                addWayPointCounter = wayPointArray.Length - 1;
                floatingButton4_clicked = true;

                Toast.MakeText(this, "Restart!", ToastLength.Short).Show();

            };
        }

        private void ShowFabMenu()
        {
            isFabOpen = true;
            floatingButton2.Visibility = ViewStates.Visible;
            floatingButton3.Visibility = ViewStates.Visible;
            floatingButton4.Visibility = ViewStates.Visible;

            floatingButton1.Animate().Rotation(135f);
            floatingButton2.Animate()
                .TranslationY(-270)
                .Rotation(360f);
            floatingButton3.Animate()
                .TranslationY(-380)
                .Rotation(360f);
            floatingButton4.Animate()
                .TranslationY(-160)
                .Rotation(360f);
        }

        private void CloseFabMenu()
        {
            isFabOpen = false;

            floatingButton1.Animate().Rotation(0f);
            floatingButton2.Animate()
                .TranslationY(0f)
                .Rotation(90f);
            floatingButton3.Animate()
                .TranslationY(0f)
                .Rotation(90f);
            floatingButton4.Animate()
                .TranslationY(0f)
                .Rotation(180f).SetListener(new FabAnimatorListener(floatingButton2, floatingButton3, floatingButton4));
        }

        private class FabAnimatorListener : Java.Lang.Object, Animator.IAnimatorListener
        {
            View[] viewsToHide;

            public FabAnimatorListener(params View[] viewsToHide)
            {
                this.viewsToHide = viewsToHide;
            }

            public void OnAnimationCancel(Animator animation)
            {
            }

            public void OnAnimationEnd(Animator animation)
            {
                if (!isFabOpen)
                    foreach (var view in viewsToHide)
                        view.Visibility = ViewStates.Gone;
            }

            public void OnAnimationRepeat(Animator animation)
            {
            }

            public void OnAnimationStart(Animator animation)
            {
            }
        }

        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null)
                throw new Exception("No Bluetooth adapter found.");

            if (!adapter.IsEnabled)
                throw new Exception("Bluetooth adapter is not enabled.");
            BluetoothDevice device = (from bd in adapter.BondedDevices
                                      where bd.Name == "HC-06" //**************bluetoothdevice name********************//TODO//
                                      select bd).FirstOrDefault();

            if (device == null)
                throw new Exception("Named device not found.");

            if (_socket == null)
            {
                _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));

                await _socket.ConnectAsync();

                connectButton.SetImageResource(Android.Resource.Drawable.IcPopupSync);


                RunOnUiThread((Action)(async () =>
                {
                    while (true)
                    {
                        await _socket.InputStream.ReadAsync(buffer, 0, buffer.Length);
                        if (received_data == 0) received_data = 1;

                        if (buffer[0] >= 0) receive_buffer[receive_buffer_counter] = buffer[0];// (byte)nextByte;                 //Load them in the received_buffer array.
                                                                                               //Search for the start signature in the received data stream.
                        if (receive_byte_previous == 'J' && receive_buffer[receive_buffer_counter] == 'B')
                        {
                            receive_buffer_counter = 0;                                           //Reset the receive_buffer_counter counter if the start signature if found.
                            receive_start_detect++;                                              //Increment the receive_start_detect to check for a full data stream reception.
                            if (receive_start_detect >= 2)
                            {
                                get_data();
                                latLongJMCdrone = new LatLng(-l_lat_gps / 1000000.0, -l_lon_gps / 1000000.0);
                                if (start == 0) wayPointArray[0] = latLongJMCdrone;
                                if (home_gps_set == 0 && number_used_sats > minSats && start == 2)
                                {
                                    home_gps_set = 1;
                                    home_lat_gps = l_lat_gps;
                                    home_lon_gps = l_lon_gps;
                                    homeLatLong = new LatLng(-home_lat_gps / 1000000.0, -home_lon_gps / 1000000.0);
                                    wayPointArray[0] = homeLatLong;
                                }
                                if (home_gps_set == 1 && start == 0)
                                {
                                    home_gps_set = 0;
                                }
                                if (fixCameraHomePoint / fixCameraHomePointDivisor != 1 || boolDragEnd || floatingButton2_clicked || floatingButton3_clicked || floatingButton4_clicked || number_used_sats == 0 || flight_mode >= 5)
                                {
                                    mapFragment.GetMapAsync(this);//update Map
                                }
                                if (number_used_sats == 0)
                                {
                                    Toast.MakeText(this, "No GPS signal", ToastLength.Long).Show();
                                    progressBar1.SetProgress(1 + myRandowInt.Next(2, 5), false);
                                    fixCameraHomePointDivisor = 1;
                                    fixCameraHomePoint = 2;
                                }
                                if (number_used_sats > 0 && number_used_sats <= minSats)
                                {
                                    progressBar1.SetProgress(number_used_sats * 7 + myRandowInt.Next(0, 5), false);
                                    fixCameraHomePoint = 6;
                                }
                                if (number_used_sats > minSats)
                                {
                                    fixCameraHomePoint = 7;

                                    progressBar1.SetProgress(80 + myRandowInt.Next(5, 20), false);
                                }

                                if (flight_mode == 1) bluetoothData1.Text = "1-Auto level";
                                if (flight_mode == 2) bluetoothData1.Text = "2-Altitude hold";
                                if (flight_mode == 3) bluetoothData1.Text = "3-GPS hold";
                                if (flight_mode == 4) bluetoothData1.Text = "4-RTH active";
                                if (flight_mode == 5) bluetoothData1.Text = "5-RTH I";   //5-RTH Increase altitude
                                if (flight_mode == 6) bluetoothData1.Text = "6-RTH R";   //6-RTH Returning to home position
                                if (flight_mode == 7) bluetoothData1.Text = "7-RTH L";   //7-RTH Landing
                                if (flight_mode == 8) bluetoothData1.Text = "8-RTH F";   //8-RTH finished
                                if (flight_mode == 9) bluetoothData1.Text = "9-Fly waypoint";

                                bluetoothData2.Text = (this.latLongJMCdrone.Latitude).ToString();
                                bluetoothData3.Text = (this.latLongJMCdrone.Longitude).ToString();
                                bluetoothData4.Text = number_used_sats.ToString();

                                bluetoothData5.Text = start.ToString();
                                bluetoothData6.Text = actual_compass_heading.ToString();
                                bluetoothData7.Text = battery_voltage.ToString();
                                bluetoothData8.Text = los_distance.ToString("0.") + " m";
                                fixCameraHomePointDivisor = fixCameraHomePoint;
                            }
                        }
                        else
                        {                                                                   //If there is no start signature detected.
                            receive_byte_previous = receive_buffer[receive_buffer_counter];       //Safe the current received byte for the next loop.
                            receive_buffer_counter++;                                            //Increment the receive_buffer_counter variable.
                            if (receive_buffer_counter > 48) receive_buffer_counter = 0;            //Reset the receive_buffer_counter variable when it becomes larger than 38.
                        }

                        if (flight_mode == 9) { fixWayPoint = true; }
                        if (flight_mode == 3 && fixWayPoint == false) await _socket.OutputStream.WriteAsync(send_buffer, 0, 13);
                        if (start == 2 && flight_mode == 3 && flyWayPointCounter <= addWayPointCounter + 1 && floatingButton3_clicked && fixWayPoint)
                        {
                            int latitude;
                            int longitude;
                            fixWayPoint = false;
                            flyWayPointCounter++;
                            floatingButton1.Hide();
                            floatingButton2.Hide();
                            floatingButton3.Hide();
                            floatingButton4.Hide();
                            if (flyWayPointCounter == addWayPointCounter + 1)
                            {  // +1 para retornar a origem !!!!
                                floatingButton3_clicked = false;
                                flyWayPointCounter = 0;

                                floatingButton1.Show();
                                floatingButton2.Show();
                                floatingButton3.Show();
                                floatingButton4.Show();
                                latitude = (int)(homeLatLong.Latitude * 1000000.0);
                                if (latitude < 0) latitude *= -1; // deve ser um valor positivo// nao sei porque //deve ser por causa do sketch translatebyte do codigo arduino!!!! 
                                longitude = (int)(homeLatLong.Longitude * 1000000.0);
                                if (longitude > 0) longitude *= -1; // deve ser um valor negativo// nao sei porque //deve ser por causa do sketch translatebyte do codigo arduino!!!! 
                            }
                            else
                            {
                                latitude = (int)(wayPointArray[flyWayPointCounter].Latitude * 1000000.0);
                                if (latitude < 0) latitude *= -1; // deve ser um valor positivo// nao sei porque //deve ser por causa do sketch translatebyte do codigo arduino!!!! 
                                longitude = (int)(wayPointArray[flyWayPointCounter].Longitude * 1000000.0);
                                if (longitude > 0) longitude *= -1; // deve ser um valor negativo// nao sei porque //deve ser por causa do sketch translatebyte do codigo arduino!!!! 
                            }

                            send_buffer[0] = (byte)'W';
                            send_buffer[1] = (byte)'P';

                            send_buffer[5] = (byte)(latitude >> 24);
                            send_buffer[4] = (byte)(latitude >> 16);
                            send_buffer[3] = (byte)(latitude >> 8);
                            send_buffer[2] = (byte)latitude;

                            send_buffer[9] = (byte)(longitude >> 24);
                            send_buffer[8] = (byte)(longitude >> 16);
                            send_buffer[7] = (byte)(longitude >> 8);
                            send_buffer[6] = (byte)longitude;

                            send_buffer[10] = (byte)'-';
                            check_byte = 0;
                            for (temp_byte = 2; temp_byte <= 10; temp_byte++)
                            {
                                check_byte ^= send_buffer[temp_byte];
                            }
                            send_buffer[11] = check_byte;
                            await _socket.OutputStream.WriteAsync(send_buffer, 0, 13);
                        }
                    }
                }));
            }
        }
        private void get_data()
        {
            check_byte = 0;
            for (temp_byte = 0; temp_byte <= 30; temp_byte++) check_byte ^= receive_buffer[temp_byte];
            if (check_byte == receive_buffer[31])
            {
                first_receive = 1;
                last_receive = milliseconds;
                receive_start_detect = 1;

                error = receive_buffer[0];
                flight_mode = receive_buffer[1];
                battery_voltage = receive_buffer[2] / 10.0f;
                battery_bar_level = receive_buffer[2];

                temperature = (short)(receive_buffer[3] | receive_buffer[4] << 8);
                roll_angle = receive_buffer[5] - 100;
                pitch_angle = receive_buffer[6] - 100;
                start = receive_buffer[7];
                altitude_meters = (receive_buffer[8] | receive_buffer[9] << 8) - 1000;
                if (altitude_meters > max_altitude_meters) max_altitude_meters = altitude_meters;
                takeoff_throttle = receive_buffer[10] | receive_buffer[11] << 8;
                actual_compass_heading = receive_buffer[12] | receive_buffer[13] << 8;
                heading_lock = receive_buffer[14];
                number_used_sats = receive_buffer[15];
                fix_type = receive_buffer[16];
                l_lat_gps = (int)receive_buffer[17] | (int)receive_buffer[18] << 8 | (int)receive_buffer[19] << 16 | (int)receive_buffer[20] << 24;
                l_lon_gps = (int)receive_buffer[21] | (int)receive_buffer[22] << 8 | (int)receive_buffer[23] << 16 | (int)receive_buffer[24] << 24;

                adjustable_setting_1 = (float)(receive_buffer[25] | receive_buffer[26] << 8) / 100.0f;
                adjustable_setting_2 = (float)(receive_buffer[27] | receive_buffer[28] << 8) / 100.0f;
                adjustable_setting_3 = (float)(receive_buffer[29] | receive_buffer[30] << 8) / 100.0f;
                ground_distance = Math.Pow((float)((l_lat_gps - home_lat_gps) ^ 2) * 0.111, 2);
                ground_distance += Math.Pow((float)(l_lon_gps - home_lon_gps) * (Math.Cos((l_lat_gps / 1000000) * 0.017453) * 0.111), 2);
                ground_distance = Math.Sqrt(ground_distance);
                los_distance = Math.Sqrt(Math.Pow(ground_distance, 2) + Math.Pow(altitude_meters, 2));


            }
        }
        public void OnMapReady(GoogleMap googleMap)
        {
            JMCdrone
                .SetPosition(latLongJMCdrone)
                .SetTitle("YMFCdrone")
                .SetSnippet(latLongJMCdrone.ToString())
                .SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.JMCdroneIcon));

            if (addWayPointCounter == 0 || number_used_sats == 0)
            {
                googleMap.Clear();
                googleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(latLongJMCdrone, 0));
                jmcMarker = googleMap.AddMarker(JMCdrone);
            }

            if (floatingButton2_clicked && addWayPointCounter < wayPointArray.Length - 1)
            {
                floatingButton2_clicked = false;
                if (number_used_sats <= minSats)
                {
                    addWayPointCounter = 0;
                    googleMap.Clear();
                    jmcMarker = googleMap.AddMarker(JMCdrone);
                    floatingButton2_clicked_counter = 0;
                    Toast.MakeText(this, "not enough satelities", ToastLength.Long).Show();

                }
                else
                {
                    googleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(latLongJMCdrone, zoom));

                    if (floatingButton2_clicked_counter == 1)
                    {
                        addWayPointCounter++;

                        MarkerOptions newMarker = new MarkerOptions()
                            .SetPosition(latLongJMCdrone)
                            .SetSnippet(latLongJMCdrone.ToString())
                            .SetTitle("wp_" + addWayPointCounter.ToString())
                            .Draggable(true)
                            .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));


                        googleMap.AddMarker(newMarker).ShowInfoWindow();
                        wayPointArray[addWayPointCounter] = latLongJMCdrone;

                    }
                }
            }
            if (boolDragEnd)
            {
                boolDragEnd = false;
                floatingButton2_clicked_counter = 0;
                googleMap.Clear();
                jmcMarker = googleMap.AddMarker(JMCdrone);
                googleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(latLongJMCdrone, zoom));

                wPpolygon.Points.Clear();
                wPpolygon.InvokeStrokeWidth(7f);
                wPpolygon.InvokeStrokeColor(Color.DarkRed.ToArgb());
                wPpolygon.InvokeFillColor(0x330000FF);
                for (int i = 0; i < addWayPointCounter + 1; i++) wPpolygon.Add(wayPointArray[i]);
                googleMap.AddPolygon(wPpolygon);

                for (int i = 1; i < addWayPointCounter + 1; i++)
                {
                    marker
                        .SetPosition(wayPointArray[i])
                        .SetTitle("wp_" + (i).ToString())
                        .SetSnippet(((float)DistanceTo(wayPointArray[i - 1].Latitude, wayPointArray[i - 1].Longitude, wayPointArray[i].Latitude, wayPointArray[i].Longitude)).ToString("F1") + " meters")
                        .Draggable(true)
                        .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));

                    googleMap.AddMarker(marker).ShowInfoWindow();

                }
            }
            if (flight_mode <= 9 && flight_mode >= 5)
            {
                if (startMission == 0)
                {
                    googleMap.Clear();
                    googleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(latLongJMCdrone, zoom));
                    if (jmcMarker != null) jmcMarker.Remove();
                    JMCdrone
                    .SetPosition(latLongJMCdrone)
                    .SetTitle("Distance to " + "wp_" + flyWayPointCounter.ToString())
                    .SetSnippet(((float)DistanceTo(latLongJMCdrone.Latitude, latLongJMCdrone.Longitude, wayPointArray[flyWayPointCounter].Latitude, wayPointArray[flyWayPointCounter].Longitude)).ToString("F1") + " meters")
                    .SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.JMCdroneIcon));

                    jmcMarker = googleMap.AddMarker(JMCdrone);
                    jmcMarker.ShowInfoWindow();

                    home
                    .SetPosition(homeLatLong)
                    .SetTitle("HOME")
                    .SetSnippet("jmcHome")
                    .Draggable(false)
                    .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueAzure));

                    googleMap.AddMarker(home);
                    for (int i = 1; i < addWayPointCounter + 1; i++)
                    {
                        marker
                            .SetPosition(wayPointArray[i])
                            .SetTitle("waypoint" + (i).ToString())
                            .Draggable(true)
                            .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueBlue));

                        googleMap.AddMarker(marker);
                    }
                    startMission = 1;
                    floatingButton2_clicked_counter = 0;
                    wPpolygon.Points.Clear();
                    wPpolygon.InvokeStrokeWidth(1f);
                    wPpolygon.InvokeStrokeColor(Color.DarkGreen.ToArgb());
                    wPpolygon.InvokeFillColor(0x33000011);
                    for (int i = 0; i < addWayPointCounter + 1; i++) wPpolygon.Add(wayPointArray[i]);
                    googleMap.AddPolygon(wPpolygon);
                }
                else
                {
                    if (jmcMarker != null) jmcMarker.Remove();
                    JMCdrone
                   .SetPosition(latLongJMCdrone)
                   .SetTitle("Distance to " + "wp_" + flyWayPointCounter.ToString())
                   .SetSnippet(((float)DistanceTo(latLongJMCdrone.Latitude, latLongJMCdrone.Longitude, wayPointArray[flyWayPointCounter].Latitude, wayPointArray[flyWayPointCounter].Longitude)).ToString("F1") + " meters");

                    jmcMarker = googleMap.AddMarker(JMCdrone);
                    jmcMarker.ShowInfoWindow();
                }
            }
            if (addWayPointCounter == wayPointArray.Length - 1)
            {
                addWayPointCounter = 0;
                floatingButton2_clicked_counter = 0;
                flyWayPointCounter = 0;
                startMission = 0;
                googleMap.Clear();
                googleMap.AddMarker(JMCdrone);
                floatingButton4_clicked = false;
                floatingButton3.Show();
            }
            googleMap.MarkerDragEnd += GoogleMap_MarkerDragEnd;
        }


        private void GoogleMap_MarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs e)
        {
            if (e.Marker.Title.ToString() == "wp_1") wayPointArray[1] = e.Marker.Position;
            if (e.Marker.Title.ToString() == "wp_2") wayPointArray[2] = e.Marker.Position;
            if (e.Marker.Title.ToString() == "wp_3") wayPointArray[3] = e.Marker.Position;
            if (e.Marker.Title.ToString() == "wp_4") wayPointArray[4] = e.Marker.Position;
            if (e.Marker.Title.ToString() == "wp_5") wayPointArray[5] = e.Marker.Position;
            if (e.Marker.Title.ToString() == "wp_6") wayPointArray[6] = e.Marker.Position;
            if (e.Marker.Title.ToString() == "wp_7") wayPointArray[7] = e.Marker.Position;
            if (e.Marker.Title.ToString() == "wp_8") wayPointArray[8] = e.Marker.Position;

            e.Marker.Snippet = "lat/lng: " + ((float)e.Marker.Position.Latitude).ToString() + " / " + ((float)e.Marker.Position.Longitude).ToString();
            e.Marker.ShowInfoWindow();
            Toast.MakeText(this, e.Marker.Title.ToString(), ToastLength.Long).Show();
            boolDragEnd = true;
            startMission = 0;
        }
        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'm')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'm': //meters -> default
                    return dist * 1.609344 * 1000;
                case 'K': //Kilometers
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }
    }
}
