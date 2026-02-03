using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SavePointTrigger : MonoBehaviour
{
    [Header("Save Point Settings")]
    [Tooltip("이 세이브 포인트의 고유 ID")]
    public string savePointId;

    [Tooltip("연속 저장 방지를 위한 쿨다운 (초)")]
    public float saveCooldown = 5f;

    [Header("Visual Feedback")]
    [Tooltip("저장 시 활성화할 시각 효과 (선택사항)")]
    public GameObject saveEffect;

    [Tooltip("저장 완료 메시지 표시 시간")]
    public float messageDisplayTime = 2f;

    private float lastSaveTime = -Mathf.Infinity;
    private bool playerInTrigger = false;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }

        if (string.IsNullOrEmpty(savePointId))
        {
            savePointId = gameObject.name;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInTrigger = true;
        TrySave();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInTrigger = false;
    }

    private void TrySave()
    {
        if (Time.time - lastSaveTime < saveCooldown)
        {
            return;
        }

        SaveManager saveManager = FindObjectOfType<SaveManager>();
        if (saveManager == null)
        {
            Debug.LogError("SavePointTrigger: SaveManager not found!");
            return;
        }

        saveManager.SaveFromTrigger(savePointId);
        lastSaveTime = Time.time;

        if (saveEffect != null)
        {
            StartCoroutine(ShowSaveEffect());
        }

        Debug.Log($"SavePointTrigger: Game saved at {savePointId}");
    }

    private IEnumerator ShowSaveEffect()
    {
        saveEffect.SetActive(true);
        yield return new WaitForSeconds(messageDisplayTime);
        saveEffect.SetActive(false);
    }

    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
        }
    }
}
