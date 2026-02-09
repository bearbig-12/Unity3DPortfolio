using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDialogueUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float typingSpeed = 0.05f;

    private string _fullText;
    private bool _isTyping;
    private Coroutine _typingCoroutine;
    private Action _onComplete;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    public void Show(string npcName, string dialogue, Action onComplete = null)
    {
        if (root != null) root.SetActive(true);
        if (nameText != null) nameText.text = npcName;

        _fullText = dialogue;
        _onComplete = onComplete;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _typingCoroutine = StartCoroutine(TypeText());
    }

    public void Hide()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        _isTyping = false;
        _onComplete = null;

        if (root != null) root.SetActive(false);
    }

    private IEnumerator TypeText()
    {
        _isTyping = true;
        dialogueText.text = "";

        foreach (char c in _fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
        _typingCoroutine = null;

        yield return new WaitForSeconds(0.3f);
        CompleteDialogue();
    }

    private void SkipToEnd()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        _isTyping = false;
        dialogueText.text = _fullText;

        CompleteDialogue();
    }

    private void CompleteDialogue()
    {
        _onComplete?.Invoke();
        _onComplete = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isTyping)
        {
            SkipToEnd();
        }
    }
}
