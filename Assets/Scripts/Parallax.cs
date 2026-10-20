using Unity.VisualScripting;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private Transform targetcamara;
    
    [SerializeField, Range(-1,1)]
    private float ParallaxEffect = 0;

    [SerializeField]
    private float spriteLenght;

    private float initialPos;

    void Start()
    {
        initialPos = transform.position.x;
    }

    private void LateUpdate()
    {
        float parallaxdistance= targetcamara.position.x * ParallaxEffect;

        transform.position = new Vector3(initialPos + parallaxdistance, transform.position.y, transform.position.z);

        float temporaldistance = targetcamara.position.x * (1 - ParallaxEffect);

        if (temporaldistance > initialPos +spriteLenght)
        {
            initialPos += spriteLenght;
        }
        else if(temporaldistance < initialPos -spriteLenght)
        {
            initialPos -= spriteLenght;
        }
    }
    void Update()
    {
        
    }
}
