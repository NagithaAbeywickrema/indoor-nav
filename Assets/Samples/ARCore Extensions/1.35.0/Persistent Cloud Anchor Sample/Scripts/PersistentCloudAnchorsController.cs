//-----------------------------------------------------------------------
// <copyright file="PersistentCloudAnchorsController.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Controller for Persistent Cloud Anchors sample.
    /// </summary>
    public class PersistentCloudAnchorsController : MonoBehaviour
    {
        [Header("AR Foundation")]

        /// <summary>
        /// The active ARSessionOrigin used in the example.
        /// </summary>
        public ARSessionOrigin SessionOrigin;

        /// <summary>
        /// The ARSession used in the example.
        /// </summary>
        public ARSession SessionCore;

        /// <summary>
        /// The ARCoreExtensions used in the example.
        /// </summary>
        public ARCoreExtensions Extensions;

        /// <summary>
        /// The active ARAnchorManager used in the example.
        /// </summary>
        public ARAnchorManager AnchorManager;

        /// <summary>
        /// The active ARPlaneManager used in the example.
        /// </summary>
        public ARPlaneManager PlaneManager;

        /// <summary>
        /// The active ARRaycastManager used in the example.
        /// </summary>
        public ARRaycastManager RaycastManager;

        public FirebaseManager FirebaseManager;

        [Header("UI")]

        /// <summary>
        /// The home page to choose entering hosting or resolving work flow.
        /// </summary>
        public GameObject HomePage;

        public string userType;

        /// <summary>
        /// The resolve screen that provides the options on which Cloud Anchors to be resolved.
        /// </summary>
        public GameObject ResolveMenu;

        /// <summary>
        /// The resolve screen that provides the options to pair hosted Cloud Anchors.
        /// </summary>
        public GameObject PairMenu;

        public GameObject LoginMenu;

        /// <summary>
        /// The information screen that displays useful information about privacy prompt.
        /// </summary>
        public GameObject PrivacyPrompt;

        /// <summary>
        /// The AR screen which displays the AR view, hosts or resolves cloud anchors,
        /// and returns to home page.
        /// </summary>
        public GameObject ARView;

        public Button HostButton;
        public Button PairButton;

        /// <summary>
        /// The current application mode.
        /// </summary>
        [HideInInspector]
        public ApplicationMode Mode = ApplicationMode.Ready;

        /// <summary>
        /// A list of Cloud Anchors that will be used in resolving.
        /// </summary>
        public HashSet<string> ResolvingSet = new HashSet<string>();

        /// <summary>
        /// The key name used in PlayerPrefs which indicates whether the start info has displayed
        /// at least one time.
        /// </summary>
        private const string _hasDisplayedStartInfoKey = "HasDisplayedStartInfo";

        /// <summary>
        /// The key name used in PlayerPrefs which stores persistent Cloud Anchors history data.
        /// Expired data will be cleared at runtime.
        /// </summary>
        private const string _persistentCloudAnchorsStorageKey = "PersistentCloudAnchors";

        private const string _pairsStorageKey = "Pairs";

        /// <summary>
        /// The limitation of how many Cloud Anchors can be stored in local storage.
        /// </summary>
        private const int _storageLimit = 40;

        private Color _activeColor;

        /// <summary>
        /// Sample application modes.
        /// </summary>
        public enum ApplicationMode
        {
            /// <summary>
            /// Ready to host or resolve.
            /// </summary>
            Ready,

            /// <summary>
            /// Hosting Cloud Anchors.
            /// </summary>
            Hosting,

            /// <summary>
            /// Resolving Cloud Anchors.
            /// </summary>
            Resolving,
        }

        /// <summary>
        /// Gets the current main camera.
        /// </summary>
        public Camera MainCamera
        {
            get
            {
                return SessionOrigin.camera;
            }
        }

        /// <summary>
        /// Callback handling "Begin to host" button click event in Home Page.
        /// </summary>
        public void OnHostButtonClicked()
        {
            Mode = ApplicationMode.Hosting;
            SwitchToPrivacyPrompt();
        }

        /// <summary>
        /// Callback handling "Pair Waypoints" button click event in Home Page.
        /// </summary>
        public void OnPairButtonClicked()
        {
            Mode = ApplicationMode.Ready;
            SwitchToPairMenu();
        }

        /// <summary>
        /// Callback handling "Begin to resolve" button click event in Home Page.
        /// </summary>
        public void OnResolveButtonClicked()
        {
            Mode = ApplicationMode.Resolving;
            SwitchToResolveMenu();
        }

        /// <summary>
        /// Callback handling "Learn More" Button click event in Privacy Prompt.
        /// </summary>
        public void OnLearnMoreButtonClicked()
        {
            Application.OpenURL(
                "https://developers.google.com/ar/data-privacy");
        }

        public void OnClearButtonClicked()
        {
            PlayerPrefs.DeleteAll();
        }

        public void OnSignoutButtonClicked()
        {
            FirebaseManager.SignOut();
            SwitchToLoginPage();
        }

        /// <summary>
        /// Switch to home page, and disable all other screens.
        /// </summary>
        public void SwitchToHomePage()
        {
            if (FirebaseManager.user == null)
            {
                HostButton.GetComponent<Image>().color = false ? _activeColor : Color.grey;
                PairButton.GetComponent<Image>().color = false ? _activeColor : Color.grey;
                HostButton.enabled = false;
                PairButton.enabled = false;
            }
            else
            {
                HostButton.GetComponent<Image>().color = true ? _activeColor : Color.grey;
                PairButton.GetComponent<Image>().color = true ? _activeColor : Color.grey;
                HostButton.enabled = true;
                PairButton.enabled = true;
            }
            ResetAllViews();
            Mode = ApplicationMode.Ready;
            ResolvingSet.Clear();
            HomePage.SetActive(true);
        }

        public void SwitchToLoginPage()
        {
            ResetAllViews();
            Mode = ApplicationMode.Ready;
            ResolvingSet.Clear();
            LoginMenu.SetActive(true);
        }

        /// <summary>
        /// Switch to resolve menu, and disable all other screens.
        /// </summary>
        public void SwitchToResolveMenu()
        {
            ResetAllViews();
            ResolveMenu.SetActive(true);
        }

        /// <summary>
        /// Switch to privacy prompt, and disable all other screens.
        /// </summary>
        public void SwitchToPrivacyPrompt()
        {
            if (PlayerPrefs.HasKey(_hasDisplayedStartInfoKey))
            {
                SwitchToARView();
                return;
            }

            ResetAllViews();
            PrivacyPrompt.SetActive(true);
        }

        /// <summary>
        /// Switch to privacy prompt, and disable all other screens.
        /// </summary>
        public void SwitchToPairMenu()
        {
            ResetAllViews();
            PairMenu.SetActive(true);
        }

        /// <summary>
        /// Switch to AR view, and disable all other screens.
        /// </summary>
        public void SwitchToARView()
        {
            ResetAllViews();
            PlayerPrefs.SetInt(_hasDisplayedStartInfoKey, 1);
            ARView.SetActive(true);
            SetPlatformActive(true);
        }

        public PairHistoryCollection LoadPairHistory()
        {
            if (PlayerPrefs.HasKey(_pairsStorageKey))
            {
                var history = JsonUtility.FromJson<PairHistoryCollection>(
                    PlayerPrefs.GetString(_pairsStorageKey));


                return history;
            }

            return new PairHistoryCollection();
        }


        public void SavePairHistory(PairHistory data)
        {
            var history = LoadPairHistory();

            // Sort the data from latest record to oldest record which affects the option order in
            // multiselection dropdown.
            history.Collection.Add(data);
            //history.Collection.Sort((left, right) => right.CreatedTime.CompareTo(left.CreatedTime));

            // Remove the oldest data if the capacity exceeds storage limit.
            if (history.Collection.Count > _storageLimit)
            {
                history.Collection.RemoveRange(
                    _storageLimit, history.Collection.Count - _storageLimit);
            }

            PlayerPrefs.SetString(_pairsStorageKey, JsonUtility.ToJson(history));
        }

        /// <summary>
        /// Load the persistent Cloud Anchors history from local storage,
        /// also remove outdated records and update local history data. 
        /// </summary>
        /// <returns>A collection of persistent Cloud Anchors history data.</returns>
        public CloudAnchorHistoryCollection LoadCloudAnchorHistory()
        {
            if (PlayerPrefs.HasKey(_persistentCloudAnchorsStorageKey))
            {
                var history = JsonUtility.FromJson<CloudAnchorHistoryCollection>(
                    PlayerPrefs.GetString(_persistentCloudAnchorsStorageKey));

                // Remove all records created more than 24 hours and update stored history.
                DateTime current = DateTime.Now;
                history.Collection.RemoveAll(
                    data => current.Subtract(data.CreatedTime).Days > 0);
                PlayerPrefs.SetString(_persistentCloudAnchorsStorageKey,
                    JsonUtility.ToJson(history));
                return history;
            }

            return new CloudAnchorHistoryCollection();
        }

        /// <summary>
        /// Save the persistent Cloud Anchors history to local storage,
        /// also remove the oldest data if current storage has met maximal capacity.
        /// </summary>
        /// <param name="data">The Cloud Anchor history data needs to be stored.</param>
        public void SaveCloudAnchorHistory(CloudAnchorHistory data)
        {
            var history = LoadCloudAnchorHistory();

            // Sort the data from latest record to oldest record which affects the option order in
            // multiselection dropdown.
            history.Collection.Add(data);
            history.Collection.Sort((left, right) => right.CreatedTime.CompareTo(left.CreatedTime));

            // Remove the oldest data if the capacity exceeds storage limit.
            if (history.Collection.Count > _storageLimit)
            {
                history.Collection.RemoveRange(
                    _storageLimit, history.Collection.Count - _storageLimit);
            }

            PlayerPrefs.SetString(_persistentCloudAnchorsStorageKey, JsonUtility.ToJson(history));
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            // Lock screen to portrait.
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.Portrait;

            _activeColor = PairButton.GetComponent<Image>().color;

            // Enable Persistent Cloud Anchors sample to target 60fps camera capture frame rate
            // on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;
            if(FirebaseManager.user != null)
            {
                SwitchToHomePage();
            }
            else
            {
                SwitchToLoginPage();
            }
            
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // On home page, pressing 'back' button quits the app.
            // Otherwise, returns to home page.
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (HomePage.activeSelf)
                {
                    Application.Quit();
                }
                else
                {
                    SwitchToHomePage();
                }
            }
        }

        private void ResetAllViews()
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            SetPlatformActive(false);
            ARView.SetActive(false);
            PrivacyPrompt.SetActive(false);
            ResolveMenu.SetActive(false);
            PairMenu.SetActive(false);
            LoginMenu.SetActive(false);
            HomePage.SetActive(false);
        }

        private void SetPlatformActive(bool active)
        {
            SessionOrigin.gameObject.SetActive(active);
            SessionCore.gameObject.SetActive(active);
            Extensions.gameObject.SetActive(active);
        }
    }
}
