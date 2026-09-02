using UnityEngine;

[CreateAssetMenu(menuName = "DestopDunge/Character Data", fileName = "NewCharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;
    public CharacterClass characterClass = CharacterClass.Monster;
    [TextArea] public string description;

    [Header("Tamagotchi (Desktop)")]
    [Tooltip("Prefab สำหรับไข่/ฟัก")]
    public GameObject desktopEggPrefab;
    [Tooltip("Prefab ระดับเล็กสำหรับโหมด Tamagotchi (2D หรือ 3D Low-poly)")]
    public GameObject desktopPrefab;
    public Sprite desktopIcon;
    [Tooltip("เวลาที่ใช้ในการฟักไข่ (วินาที)")]
    public float hatchTime = 60f;
    [Tooltip("เวลาที่ใช้ในการเติบโต/เปลี่ยนขั้น (วินาที)")]
    public float growthTime = 300f;
    [Tooltip("ความจุพลังงานสูงสุด")]
    public float energyCapacity = 100f;
    [Tooltip("อัตราการสะสมพลังงานต่อวินาที (Passive)")]
    public float energyPerSecond = 1f;
    public ParticleSystem auraVFX;

    [Header("Hack & Slash (Combat)")]
    [Tooltip("Prefab ร่างต่อสู้เต็มตัว (Full 3D)")]
    public GameObject combatPrefab;
    [Tooltip("Animator Controller สำหรับการโจมตี/คอมแบท")]
    public RuntimeAnimatorController combatAnimatorController;

    [Header("Base Stats")]
    public int maxHP = 100;
    public int attack = 10;
    [Tooltip("จำนวนครั้งโจมตีต่อวินาที")]
    public float attackSpeed = 1.0f;
    public float moveSpeed = 3f;
    public bool hasSuperArmor = false;

    [Header("Combat VFX / SFX")]
    public GameObject swingVFX;
    public GameObject hitVFX;
    public GameObject deathVFX;
    public AudioClip attackSFX;
    public AudioClip transformSFX;

    [Header("Combat extras")]
    public float screenShakeOnHeavy = 0.25f;
    public float knockbackForce = 5f;

    [Header("Upgrade / Economy (optional)")]
    public int unlockCost = 0;
    public int[] upgradeCosts;
}

public enum CharacterClass
{
    Monster,
    SexySpellblade,
    HeavyValkyrie
}
