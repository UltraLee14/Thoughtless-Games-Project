using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeybindMapManager : MonoBehaviour
{
    [SerializeField, InspectorName("Player Stats Object")]
    PlayerStats playerStatsObject;

    [Header("UI Buttons (Optional Auto-Refresh)")]
    [SerializeField, InspectorName("Action Buttons")]
    GameObject[] actionButtons = new GameObject[0];

    [Header("Button Naming")]
    [SerializeField, InspectorName("Button Suffix")]
    string remapButtonSuffix = "RemapButton";

    [Header("Events")]
    [SerializeField, InspectorName("Rebind Started")]
    UnityEvent rebindStarted = new UnityEvent();

    [SerializeField, InspectorName("Rebind Completed")]
    UnityEvent rebindCompleted = new UnityEvent();

    [SerializeField, InspectorName("Rebind Canceled")]
    UnityEvent rebindCanceled = new UnityEvent();

    [Header("Debug Readouts")]
    [SerializeField, InspectorName("Is Listening")]
    bool isListening;

    [SerializeField, InspectorName("Listening Action")]
    string listeningAction;

    [SerializeField, InspectorName("Last Bound Key")]
    string lastBoundKey;

    int listeningIndex = -1;
    GameObject listeningButton;

    void Awake()
    {
        EnsureDefaultControlsExist();
    }

    void Start()
    {
        RefreshAllButtonLabels();
    }

    public void RefreshAllButtonLabels()
    {
        if (actionButtons == null) return;

        for (int i = 0; i < actionButtons.Length; i++)
        {
            var b = actionButtons[i];
            if (b == null) continue;

            string actionName = GetActionNameFromButton(b.name);
            var key = GetKey(actionName);

            SetButtonLabel(b, FormatKey(key));
        }
    }

    public void BeginRebindFromButton(GameObject buttonObject)
    {
        if (buttonObject == null) return;

        string actionName = GetActionNameFromButton(buttonObject.name);
        listeningButton = buttonObject;

        BeginRebindByActionName(actionName);

        if (isListening)
            SetButtonLabel(buttonObject, "...");
    }

    public void BeginRebindByIndex(int controlValueIndex)
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.controlValues == null) return;
        if (controlValueIndex < 0 || controlValueIndex >= playerStatsObject.controlValues.Length) return;

        var cv = playerStatsObject.controlValues[controlValueIndex];
        if (cv == null) return;
        if (!cv.isKeybind) return;

        listeningIndex = controlValueIndex;
        listeningAction = cv.actionName;
        isListening = true;

        rebindStarted.Invoke();
    }

    public void BeginRebindByActionName(string actionName)
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.controlValues == null) return;
        if (string.IsNullOrWhiteSpace(actionName)) return;

        for (int i = 0; i < playerStatsObject.controlValues.Length; i++)
        {
            var cv = playerStatsObject.controlValues[i];
            if (cv == null) continue;
            if (!cv.isKeybind) continue;

            if (string.Equals(cv.actionName, actionName, StringComparison.OrdinalIgnoreCase))
            {
                BeginRebindByIndex(i);
                return;
            }
        }
    }

    public void CancelRebind()
    {
        if (!isListening) return;

        var prevButton = listeningButton;
        string prevAction = listeningAction;

        isListening = false;
        listeningIndex = -1;
        listeningAction = "";
        listeningButton = null;

        if (prevButton != null)
        {
            var key = GetKey(prevAction);
            SetButtonLabel(prevButton, FormatKey(key));
        }

        rebindCanceled.Invoke();
    }

    public KeyCode GetKey(string actionName)
    {
        if (playerStatsObject == null) return KeyCode.None;
        if (playerStatsObject.controlValues == null) return KeyCode.None;
        if (string.IsNullOrWhiteSpace(actionName)) return KeyCode.None;

        for (int i = 0; i < playerStatsObject.controlValues.Length; i++)
        {
            var cv = playerStatsObject.controlValues[i];
            if (cv == null) continue;
            if (!cv.isKeybind) continue;

            if (string.Equals(cv.actionName, actionName, StringComparison.OrdinalIgnoreCase))
                return cv.boundKey;
        }

        return KeyCode.None;
    }

    public void ResetAllToDefaults()
    {
        if (playerStatsObject == null) return;
        if (playerStatsObject.controlValues == null) return;

        for (int i = 0; i < playerStatsObject.controlValues.Length; i++)
        {
            var cv = playerStatsObject.controlValues[i];
            if (cv == null) continue;
            if (!cv.isKeybind) continue;

            cv.boundKey = cv.defaultKey;
        }

        RefreshAllButtonLabels();
    }

    void OnGUI()
    {
        if (!isListening) return;

        if (Event.current != null && Event.current.type == EventType.KeyDown)
        {
            KeyCode k = (KeyCode)Event.current.keyCode;

            if (k == KeyCode.Escape)
            {
                CancelRebind();
                Event.current.Use();
                return;
            }
        }

        if (playerStatsObject == null) return;
        if (playerStatsObject.controlValues == null) return;
        if (listeningIndex < 0 || listeningIndex >= playerStatsObject.controlValues.Length) return;

        Event e = Event.current;
        if (e == null) return;

        if (e.type == EventType.KeyDown)
        {
            SetBinding((KeyCode)e.keyCode);
            e.Use();
            return;
        }

        if (Input.GetMouseButtonDown(0)) { SetBinding(KeyCode.Mouse0); return; }
        if (Input.GetMouseButtonDown(1)) { SetBinding(KeyCode.Mouse1); return; }
        if (Input.GetMouseButtonDown(2)) { SetBinding(KeyCode.Mouse2); return; }
    }

    void SetBinding(KeyCode key)
    {
        if (key == KeyCode.Escape)
        {
            CancelRebind();
            return;
        }

        if (playerStatsObject == null) return;
        if (playerStatsObject.controlValues == null) return;
        if (listeningIndex < 0 || listeningIndex >= playerStatsObject.controlValues.Length) return;

        var cv = playerStatsObject.controlValues[listeningIndex];
        if (cv == null) return;

        cv.boundKey = key;
        lastBoundKey = FormatKey(key);

        var finishedButton = listeningButton;

        isListening = false;
        listeningIndex = -1;
        listeningAction = "";
        listeningButton = null;

        if (finishedButton != null)
            SetButtonLabel(finishedButton, FormatKey(key));

        RefreshAllButtonLabels();
        rebindCompleted.Invoke();
    }

    string GetActionNameFromButton(string buttonName)
    {
        if (string.IsNullOrWhiteSpace(buttonName)) return "";

        string n = buttonName.Trim();
        if (!string.IsNullOrEmpty(remapButtonSuffix) &&
            n.EndsWith(remapButtonSuffix, StringComparison.OrdinalIgnoreCase))
        {
            n = n.Substring(0, n.Length - remapButtonSuffix.Length);
        }

        return n.Trim();
    }

    string FormatKey(KeyCode key)
    {
        if (key == KeyCode.None) return "";

        if (key == KeyCode.LeftShift) return "L Shift";
        if (key == KeyCode.RightShift) return "R Shift";
        if (key == KeyCode.LeftControl) return "L Ctrl";
        if (key == KeyCode.RightControl) return "R Ctrl";
        if (key == KeyCode.LeftAlt) return "L Alt";
        if (key == KeyCode.RightAlt) return "R Alt";

        if (key == KeyCode.Mouse0) return "Mouse 0";
        if (key == KeyCode.Mouse1) return "Mouse 1";
        if (key == KeyCode.Mouse2) return "Mouse 2";

        string s = key.ToString();
        s = s.Replace("Alpha", "");
        return s;
    }

    void SetButtonLabel(GameObject buttonObject, string text)
    {
        if (buttonObject == null) return;

        var tmp = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = text;
            return;
        }

        var uiText = buttonObject.GetComponentInChildren<Text>(true);
        if (uiText != null)
        {
            uiText.text = text;
        }
    }

    void EnsureDefaultControlsExist()
    {
        if (playerStatsObject == null) return;

        EnsureAction("Move Forward", KeyCode.W);
        EnsureAction("Move Backward", KeyCode.S);
        EnsureAction("Move Left", KeyCode.A);
        EnsureAction("Move Right", KeyCode.D);
        EnsureAction("Sprint", KeyCode.LeftShift);
        EnsureAction("Sneak", KeyCode.LeftControl);
        EnsureAction("Jump", KeyCode.Space);
        EnsureAction("Interact", KeyCode.F);
        EnsureAction("Echo Locate", KeyCode.R);
    }

    void EnsureAction(string actionName, KeyCode defaultKey)
    {
        if (playerStatsObject.controlValues == null)
            playerStatsObject.controlValues = new PlayerStats.ControlValue[0];

        for (int i = 0; i < playerStatsObject.controlValues.Length; i++)
        {
            var cv = playerStatsObject.controlValues[i];
            if (cv == null) continue;

            if (string.Equals(cv.actionName, actionName, StringComparison.OrdinalIgnoreCase))
            {
                if (cv.defaultKey == KeyCode.None) cv.defaultKey = defaultKey;
                if (cv.boundKey == KeyCode.None) cv.boundKey = defaultKey;
                cv.isKeybind = true;
                return;
            }
        }

        int oldLen = playerStatsObject.controlValues.Length;
        var newArr = new PlayerStats.ControlValue[oldLen + 1];
        for (int i = 0; i < oldLen; i++) newArr[i] = playerStatsObject.controlValues[i];

        var added = new PlayerStats.ControlValue();
        added.actionName = actionName;
        added.defaultKey = defaultKey;
        added.boundKey = defaultKey;
        added.isKeybind = true;

        newArr[oldLen] = added;
        playerStatsObject.controlValues = newArr;
    }
}
