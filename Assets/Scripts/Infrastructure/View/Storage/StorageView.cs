using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StorageView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stoneText;
    [SerializeField] private TextMeshProUGUI _woodText;
    [SerializeField] private TextMeshProUGUI _ironText;

    private Dictionary<ItemDeliverableType, TextMeshProUGUI> _textByItemType;

    private void Awake()
    {
        Initialize();
    }

    public void ChangeResourceText(ItemDeliverableType deliverableType, int itemCount)
    {
        ThrowIf.Invalid(deliverableType == ItemDeliverableType.Unknown, $"{nameof(ItemDeliverableType)} can't be {deliverableType} {nameof(ItemDeliverableType)}");

        _textByItemType[deliverableType].text = itemCount.ToString();
    }

    private void Initialize()
    {
        _textByItemType = new()
        {
            { ItemDeliverableType.Stone, _stoneText },
            { ItemDeliverableType.Wood, _woodText },
            { ItemDeliverableType.Iron, _ironText },
        };
    }
}