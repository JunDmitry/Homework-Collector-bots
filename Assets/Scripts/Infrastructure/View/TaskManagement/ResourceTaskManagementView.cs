using TMPro;
using UnityEngine;

public class ResourceTaskManagementView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _readyToAssignCountText;
    [SerializeField] private TextMeshProUGUI _assignTaskCountText;
    [SerializeField] private TextMeshProUGUI _completeTaskCountText;

    public void ChangeTaskManagementInfo(IReadonlyTaskManagementInfo taskManagementInfo)
    {
        _readyToAssignCountText.text = taskManagementInfo.ReadyToAssignCount.ToString();
        _assignTaskCountText.text = taskManagementInfo.AssignCount.ToString();
        _completeTaskCountText.text = taskManagementInfo.CompletedCount.ToString();
    }
}