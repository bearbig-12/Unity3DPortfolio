using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button closeButton;

    private System.Action _onAccept;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        if (acceptButton != null) acceptButton.onClick.AddListener(() =>
        {
            _onAccept?.Invoke();
            Hide();
        });
    }

    public void Show(string title, string text, System.Action onAccept = null, bool showAccept = false)
    {
        Debug.Log($"DialogueUI.Show called. root null? {root == null}");
        if (root != null) root.SetActive(true);
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = text;

        _onAccept = onAccept;

        if (acceptButton != null)
            acceptButton.gameObject.SetActive(showAccept);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        _onAccept = null;
    }
}