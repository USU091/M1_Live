using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class InventoryManager
{
    public readonly int DEFAULT_INVENTORY_SLOT_COUNT = 30;

    public List<Item> AllItems { get; } = new List<Item>();

    //Cache
    Dictionary<int, Item> EquippedItems = new Dictionary<int, Item>();  // ��� �κ�
    List<Item> InventoryItems = new List<Item>();
    List<Item> WarehouseItems = new List<Item>();



    //ó������ �������� ����� �Լ�/ SaveFile�� �ƴ�  // ex. ���Ͱ� �׾ ����� ������ ���� �� ������ ��Ȳ
    public Item MakeItem(int itemTemplateId, int count = 1)
    {
        int itemDbId = Managers.Game.GenerateItemDbId();

        if (Managers.Data.ItemDic.TryGetValue(itemTemplateId, out ItemData itemdata) == false)
            return null;

        ItemSaveData saveData = new ItemSaveData()
        {
            InstanceId = itemDbId,
            DbId = itemDbId,
            TempplateId = itemTemplateId,
            Count = count,
            EquipSlot = (int)EEquipSlotType.Inventory,
            EnchantCount = 0

        };

        return AddItem(saveData);
    }


    public Item AddItem(ItemSaveData itemInfo)
    {
        Item item = Item.MakeItem(itemInfo);

        if (item == null)
            return null;

        if(item.IsEquippedItem())
        {
            EquippedItems.Add(item.SaveData.EquipSlot, item);
        }
        else if(item.IsInventory())
        {
            InventoryItems.Add(item);
        }
        else if(item.IsWarehouse())
        {
            WarehouseItems.Add(item);
        }

        AllItems.Add(item);


        return item;
    }

    public void RemoveItem(int instanceId)
    {
        Item item = AllItems.Find(x => x.SaveData.InstanceId == instanceId);
        if (item == null)
            return;

        if (item.IsEquippedItem())
        {
            EquippedItems.Remove(item.SaveData.EquipSlot);
        }
        else if (item.IsInventory())
        {
            InventoryItems.Remove(item);
        }
        else if(item.IsWarehouse())
        {
            WarehouseItems.Remove(item);
        }

        AllItems.Remove(item);
    }

    public void EquipItem(int instanceId)
    {
        Item item = InventoryItems.Find(x => x.SaveData.InstanceId == instanceId);
        if(item == null)
        {
            Debug.Log("������ ����");
            return;
        }

        EEquipSlotType equipSlotType = item.GetEquipItemEquipSlot();
        if (equipSlotType == EEquipSlotType.None)
            return;

        //���� ������ ����
        if (EquippedItems.TryGetValue((int)equipSlotType, out Item prev))
            UnEquipItem(prev.InstaceId);

        //������ ����
        item.EquipSlot = (int)equipSlotType;
        EquippedItems[(int)equipSlotType] = item;

    }

    public void UnEquipItem(int instanceId, bool checkFull = true)
    {
        var item = EquippedItems.Values.Where(x => x.InstaceId == instanceId).FirstOrDefault();
        if (item == null)
            return;

        //TODO

        if (checkFull && IsInventoryFull())
            return;

        EquippedItems.Remove((int)item.EquipSlot);

        item.EquipSlot = (int)EEquipSlotType.Inventory;
        InventoryItems.Add(item);

    }
    
    public void Clear()
    {
        AllItems.Clear();

        EquippedItems.Clear();
        InventoryItems.Clear();
        WarehouseItems.Clear();
    }



    #region Helper

    public Item GetItem(int instanceId)
    {
        return AllItems.Find(item => item.SaveData.InstanceId == instanceId);
    }

    public Item GetEquippedItem(EEquipSlotType equipSlotType)
    {
        EquippedItems.TryGetValue((int)equipSlotType, out Item item);

        return item;
    }

    public Item GetEquippedItemBySubType(EItemSubType subType)
    {
        return EquippedItems.Values.Where(x => x.SubType == subType).FirstOrDefault();
    }

    public Item GetItemInInventory(int instanceId)
    {
        return InventoryItems.Find(x => x.SaveData.InstanceId == instanceId);
    }

    public bool IsInventoryFull()
    {
        return InventoryItems.Count >= InventorySlotCount();
    }
    
    public int InventorySlotCount()
    {
        return DEFAULT_INVENTORY_SLOT_COUNT;
    }

    public List<Item> GetEquippedItems()
    {
        return EquippedItems.Values.ToList();
    }

    public List<ItemSaveData> GetEquippedItemInfos()
    {
        return EquippedItems.Values.Select(x => x.SaveData).ToList();
    }

    public List<Item> GetInventoryItems()
    {
        return InventoryItems.ToList();
    }
    public List<ItemSaveData> GetInventoryItemInfos()
    {
        return InventoryItems.Select(x => x.SaveData).ToList();
    }

    public List<ItemSaveData> GetInventoryItemInfosOrderByGrade()
    {
        return InventoryItems.OrderByDescending(y => (int)y.TemplateData.Grade)
            .ThenBy(y => (int)y.TemplateId)
            .Select(x => x.SaveData)
            .ToList();
    }

    public List<ItemSaveData> GetWarehouseItemInfos()
    {
        return WarehouseItems.Select(x => x.SaveData).ToList();
    }

    public List<ItemSaveData> GetWarehouseItemInfosOrderByGrade()
    {
        return WarehouseItems.OrderByDescending(y => (int)y.TemplateData.Grade)
            .ThenBy(y => (int)y.TemplateId)
            .Select(x => x.SaveData)
            .ToList();
    }

    #endregion
}

