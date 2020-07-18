using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Service.Notification;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.Net;
using Newtonsoft.Json;

namespace App2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Button save = FindViewById<Button>(Resource.Id.button1);
            save.Click += Save;

            Button test = FindViewById<Button>(Resource.Id.button2);
            test.Click += Test;

            Button givePermission = FindViewById<Button>(Resource.Id.button3);
            givePermission.Click += GivePermission;
        }

        public void Save(object sender, EventArgs eventArgs)
        {
            var txt = FindViewById<EditText>(Resource.Id.editText1).Text;
            Preferences.Set("URL",txt);
        }

        public void Test(object sender, EventArgs eventArgs)
        {
            Post temp = new Post("This is a test");
            temp.CallWebhook().Wait();
        }

        public void GivePermission(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent("android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS");
            StartActivity(intent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public class Post
        {
            public string username = "RustHelper";
            public string avatar_url;
            public string content;
            public object[] embeds;

            private static HttpClient client = new HttpClient();

            public Post(string content)
            {
                avatar_url = @"https://greycat.dk/Logo.png";
                this.content = content;
            }

            public async Task CallWebhook()
            {
                string temp = JsonConvert.SerializeObject(this);
                string url = Preferences.Get("URL", "");
                var result = client.PostAsync(url, new StringContent(temp, Encoding.UTF8, "application/json")).Result;
                var resultString = await result.Content.ReadAsStringAsync();
            }

        }

        [Service(Label = "RustNotificationListener", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE")]
        [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
        public class NLService : NotificationListenerService
        {
            public override void OnNotificationPosted(StatusBarNotification sbn)
            {
                base.OnNotificationPosted(sbn);
                if (sbn.PackageName == "com.facepunch.rust.companion")
                {
                    var txt = sbn.Notification.Extras.GetString("android.title") + "\n" + sbn.Notification.Extras.GetString("android.text");
                    var post = new Post(txt);
                    post.CallWebhook().Wait();
                }
            }
        }
    }
}
