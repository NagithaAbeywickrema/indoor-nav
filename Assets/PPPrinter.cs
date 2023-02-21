namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class PPPrinter : MonoBehaviour
    {
        public Text DebugText;

        private const string _persistentCloudAnchorsStorageKey = "PersistentCloudAnchors";

        private const string _pairsStorageKey = "Pairs";

        private void OnEnable()
        {
            string txt = "";
            if (PlayerPrefs.HasKey(_persistentCloudAnchorsStorageKey))
            {
                var history = JsonUtility.FromJson<CloudAnchorHistoryCollection>(
                    PlayerPrefs.GetString(_persistentCloudAnchorsStorageKey));
                foreach (var data in history.Collection)
                {
                    txt += data.Name + ", " + data.Type + ", " + data.Id + "\n";
                }
            }
            txt += "\n";
            if (PlayerPrefs.HasKey(_pairsStorageKey))
            {
                var history2 = JsonUtility.FromJson<PairHistoryCollection>(
                    PlayerPrefs.GetString(_pairsStorageKey));
                foreach (var data in history2.Collection)
                {
                    txt += data.Id1 + ", " + data.Id2 + "\n";
                }

            }
            DebugText.text = txt;
        }

    }
}