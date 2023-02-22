namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using Firebase.Auth;

    public class FirebaseManager : MonoBehaviour
    {
        // Start is called before the first frame update
        // For authentication
        public Text logText;
        public Button signInButton, signUpButton;
        public InputField email, password, verifyPassword;
        private Firebase.Auth.FirebaseAuth auth;
        public Firebase.Auth.FirebaseUser user;
        private Firebase.FirebaseApp app;
        public string displayName;
        public string emailAddress;
        public PersistentCloudAnchorsController Controller;

        void Start()
        {

            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    this.app = Firebase.FirebaseApp.DefaultInstance;
                    InitializeFirebase();
                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    Debug.Log("Error");
                    Debug.Log(dependencyStatus);
                    UnityEngine.Debug.Log(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        void InitializeFirebase()
        {
            this.auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            this.auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
        }

        void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            if (this.auth.CurrentUser != this.user)
            {
                bool signedIn = this.user != this.auth.CurrentUser && this.auth.CurrentUser != null;
                if (!signedIn && this.user != null)
                {
                    Debug.Log("Signed out " + user.UserId);
                    Controller.SwitchToLoginPage();
                }
                this.user = this.auth.CurrentUser;
                if (signedIn)
                {
                    Debug.Log("Signed in " + user.UserId);
                    this.displayName = this.user.DisplayName ?? "";
                    this.emailAddress = this.user.Email ?? "";
                    Controller.SwitchToHomePage();
                }
            }
        }

        public void OnClickSignup()
        {
            string em = email.text;
            string ps = password.text;

            this.auth.CreateUserWithEmailAndPasswordAsync(em, ps).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                // Firebase user has been created.
                Firebase.Auth.FirebaseUser newUser = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);
            });
        }

        public void OnClickSignin()
        {
            string em = email.text;
            string ps = password.text;

            this.auth.SignInWithEmailAndPasswordAsync(em, ps).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                Firebase.Auth.FirebaseUser newUser = task.Result;
                this.user = newUser;
                Controller.SwitchToHomePage();
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);
            });
        }

        public void SignOut()
        {
            if(auth != null)
            {
                this.auth.SignOut();
            }
            
        }
    }
}
