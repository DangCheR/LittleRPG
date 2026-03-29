using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class CursorManager : MonoBehaviour
{
    public Texture2D normalCursor;
    public Texture2D attackCursor;
    public Texture2D pickUpCursor;
    private EnumCursor currCursor = EnumCursor.normal;
    public enum EnumCursor
    {
        normal,
        attack,
        pickup
    }
    public void Awake()
    {
        SetNormal();
    }
    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, 10000, ~(1 << 6 | 1 << 7)))
        {
            // 当光标在UI图层上
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SetCursor(EnumCursor.normal);
                return;
            }
            if (raycastHit.transform.tag == FieldManager.EnemyTag)
            {
                SetCursor(EnumCursor.attack);
            }
            else if (raycastHit.transform.tag == "PickUpItem" || raycastHit.transform.tag == "NPC")
            {
                SetCursor(EnumCursor.pickup);
            }
            else
            {
                SetCursor(EnumCursor.normal);
            }
        }
    }
    private void SetCursor(EnumCursor c)
    {
        if (currCursor == c) return;
        switch (c)
        {
            case EnumCursor.normal:
                Cursor.SetCursor(normalCursor, new Vector2(10, 5), CursorMode.Auto);
                break;
            case EnumCursor.attack:
                Cursor.SetCursor(attackCursor, new Vector2(10, 5), CursorMode.Auto);
                break;
            case EnumCursor.pickup:
                Cursor.SetCursor(pickUpCursor, new Vector2(10, 5), CursorMode.Auto);
                break;
        }
        currCursor = c;
    }
    public void SetNormal()
    {
        Cursor.SetCursor(normalCursor, new Vector2(10, 5), CursorMode.Auto);
    }

    public void SetAttack()
    {
        Cursor.SetCursor(attackCursor, new Vector2(5, 2), CursorMode.Auto);
    }
    public void SetPickUp()
    {
        Cursor.SetCursor(pickUpCursor, new Vector2(5, 2), CursorMode.Auto);
    }
}
