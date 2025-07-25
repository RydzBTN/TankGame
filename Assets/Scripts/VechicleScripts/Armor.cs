using UnityEngine;
using UnityEngine.Events;

public class Armor : MonoBehaviour
{
    [Header("Typ pancerza")]
    [Tooltip("Wybierz typ pancerza z listy")]
    public ArmorType armorType = ArmorType.RHA;
    [Tooltip("gruboœæ w milimetrach")]
    public float armorThickness = 10f; // Gruboœæ pancerza w mm

    public float effectiveThickness; // Efektywna gruboœæ pancerza

    private int armorTypeIndex; // Indeks typu pancerza (dla ³atwego dostêpu)

    private void Start()
    {
        armorTypeIndex = (int)armorType; // Inicjalizacja indeksu typu pancerza
        effectiveThickness = armorThickness * armorTypeMultipliers[armorTypeIndex];
    }
    
 
    public enum ArmorType
    {
        RHA,            // Rolled Homogeneous Armor (standardowa stal)
        CastSteel,      // Stal odlewana
        Wood,           // Drewno
        Aluminium,      // Aluminium
    }

    // S³ownik z mno¿nikami efektywnej gruboœci dla ró¿nych typów pancerza
    private static readonly float[] armorTypeMultipliers = {
        1.0f,   // RHA (baza)
        0.9f,   // Stal odlewana (mniej efektywna ni¿ RHA)
        0.1f,   // Drewno (prawie bez ochrony)
        0.5f,   // Aluminium (œrednia ochrona)
    };

}
