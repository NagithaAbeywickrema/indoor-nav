namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A manager component that helps to populate and handle the options of resolving anchors.
    /// </summary>
    public class LoginViewManager : MonoBehaviour
    {
        /// <summary>
        /// The main controller for Persistent Cloud Anchors sample.
        /// </summary>
        public PersistentCloudAnchorsController Controller;

        /// <summary>
        /// A multiselection dropdown component that contains all available resolving options.
        /// </summary>
        public MultiselectionDropdown Multiselection;

        /// <summary>
        /// A multiselection dropdown component that contains all available resolving options.
        /// </summary>
        public MultiselectionDropdown MultiselectionNext;

        /// <summary>
        /// An input field for manually typing Cloud Anchor Id(s).
        /// </summary>
        public InputField InputField;

        /// <summary>
        /// The warning text that appears when invalid characters are filled in.
        /// </summary>
        public GameObject InvalidInputWarning;

        /// <summary>
        /// The resolve button which leads to AR view screen.
        /// </summary>
        public Button PairButton;

        /// <summary>
        /// Cached Cloud Anchor history data used to fetch the Cloud Anchor Id using
        /// the index given by multi-selection dropdown.
        /// </summary>
        private CloudAnchorHistoryCollection _history = new CloudAnchorHistoryCollection();

        /// <summary>
        /// Cached active color for interactable buttons.
        /// </summary>
        private Color _activeColor;

        private string[] pair = new string[2];

        /// <summary>
        /// Callback handling the validation of the input field.
        /// </summary>
        /// <param name="inputString">Current value of the input field.</param>
        public void OnInputFieldValueChanged(string inputString)
        {
            // Input should only contain:
            // letters, numbers, hyphen(-), underscore(_), and comma(,).
            // Note: the valid character set is controlled by the validation rule of
            // the naming field in AR View.
            var regex = new Regex("^[a-zA-Z0-9-_,]*$");
            InvalidInputWarning.SetActive(!regex.IsMatch(inputString));
        }

        /// <summary>
        /// Callback handling the end edit event of the input field.
        /// </summary>
        /// <param name="inputString">The value of the input field.</param>
        public void OnInputFieldEndEdit(string inputString)
        {
            if (InvalidInputWarning.activeSelf)
            {
                return;
            }

            OnResolvingSelectionChanged();
        }

        /// <summary>
        /// Callback handling the selection values changed in multiselection dropdown and
        /// input field.
        /// </summary>
        public void OnResolvingSelectionChanged()
        {
            Controller.ResolvingSet.Clear();

            // Add Cloud Anchor Ids from multiselection dropdown.
            List<int> selectedIndex = Multiselection.SelectedValues;
            List<int> selectedIndexNext = MultiselectionNext.SelectedValues;
            if (selectedIndex.Count > 0 && selectedIndexNext.Count > 0)
            {
                pair[0] = _history.Collection[selectedIndex[0]].Id;
                pair[1] = _history.Collection[selectedIndexNext[0]].Id;
                SetButtonActive(PairButton, true);
            }

            // Add Cloud Anchor Ids from input field.
            if (!InvalidInputWarning.activeSelf && InputField.text.Length > 0)
            {
                string[] inputIds = InputField.text.Split(',');
                if (inputIds.Length > 0)
                {
                    Controller.ResolvingSet.UnionWith(inputIds);
                }
            }
        }

        public void OnPairButtonClicked()
        {
            PairHistory pair_history = new PairHistory(pair[0], pair[1]);
            Controller.SavePairHistory(pair_history);
            Controller.SwitchToHomePage();
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _activeColor = PairButton.GetComponent<Image>().color;
        }

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            SetButtonActive(PairButton, false);
            InvalidInputWarning.SetActive(false);
            InputField.text = string.Empty;
            _history = Controller.LoadCloudAnchorHistory();

            Multiselection.OnValueChanged += OnResolvingSelectionChanged;
            MultiselectionNext.OnValueChanged += OnResolvingSelectionChanged;
            var options = new List<MultiselectionDropdown.OptionData>();
            foreach (var data in _history.Collection)
            {
                options.Add(new MultiselectionDropdown.OptionData(
                    data.Name, data.Type));
            }

            Multiselection.Options = options;
            MultiselectionNext.Options = options;
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            Multiselection.OnValueChanged -= OnResolvingSelectionChanged;
            Multiselection.Deselect();
            Multiselection.Options.Clear();
            MultiselectionNext.OnValueChanged -= OnResolvingSelectionChanged;
            MultiselectionNext.Deselect();
            MultiselectionNext.Options.Clear();
            _history.Collection.Clear();
        }

        private string FormatDateTime(DateTime time)
        {
            TimeSpan span = DateTime.Now.Subtract(time);
            return span.Hours == 0 ? span.Minutes == 0 ? "Just now" :
                string.Format("{0}m ago", span.Minutes) : string.Format("{0}h ago", span.Hours);
        }

        private void SetButtonActive(Button button, bool active)
        {
            button.GetComponent<Image>().color = active ? _activeColor : Color.grey;
            button.enabled = active;
        }
    }
}
