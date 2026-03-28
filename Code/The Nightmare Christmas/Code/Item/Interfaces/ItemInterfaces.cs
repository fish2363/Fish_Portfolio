using UnityEngine;

public interface IUseItem
{
    void Use(PlayerItemController handController);
}

public interface IPuzzleItem
{
    void Use(PlayerItemController handController, RaycastHit hit);
}