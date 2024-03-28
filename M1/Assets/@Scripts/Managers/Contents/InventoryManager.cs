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
    Dictionary<int, Item> EquippedItems = new Dictionary<int, Item>();  // 장비 인벤
    List<Item> InventoryItems = new List<Item>();
    List<Item> WarehouseItems = new List<Item>();



    //처음으로 아이템을 만드는 함수/ SaveFile이 아님  // ex. 몬스터가 죽어서 드랍한 아이템 생성 후 줍줍한 상황
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
            Debug.Log("아이템 없음");
            return;
        }

        EEquipSlotType equipSlotType = item.GetEquipItemEquipSlot();
        if (equipSlotType == EEquipSlotType.None)
            return;

        //기존 아이템 해제
        if (EquippedItems.TryGetValue((int)equipSlotType, out Item prev))
            UnEquipItem(prev.InstaceId);

        //아이템 장착
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

