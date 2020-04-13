using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AppYMFC32panel;

namespace AppYMFC32panel
{
    [Activity(Label = "RegisterActivity")]
    public class RegisterActivity : Activity
    {
        EditText emailEditText, passwordEditText, confirmPasswordEditText;
        Button registerButton;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.registerPage);

            emailEditText = FindViewById<EditText>(Resource.Id.registerEmailEditText);
            passwordEditText = FindViewById<EditText>(Resource.Id.registerPasswordEditText);
            confirmPasswordEditText = FindViewById<EditText>(Resource.Id.confirmPasswordEditText);
            registerButton = FindViewById<Button>(Resource.Id.registerUserButton);

            registerButton.Click += RegisterButton_Click;

            string email = Intent.GetStringExtra("email");
            emailEditText.Text = email;
        }

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            WebClient clientReg = new WebClient();
            Uri uri = new Uri("http://177.32.196.84:4000/appymfcpanel/register.php");
            Uri uri2 = new Uri("https://jmcdrone.000webhostapp.com/register.php");
            NameValueCollection nameValue = new NameValueCollection();
            nameValue.Add("email", emailEditText.Text);
            nameValue.Add("password", passwordEditText.Text);
            clientReg.UploadValuesAsync(uri2, nameValue);


        }
    }
}