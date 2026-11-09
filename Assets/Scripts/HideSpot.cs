using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class HideSpot : MonoBehaviour
{
    public SpriteRenderer objectSprite; // sprite del objeto que oculta al jugador

    public string SortingLayerName => objectSprite.sortingLayerName;
    public int SortingOrder => objectSprite.sortingOrder;

    void Reset()
    {
        objectSprite = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().isTrigger = true;
    }
}
