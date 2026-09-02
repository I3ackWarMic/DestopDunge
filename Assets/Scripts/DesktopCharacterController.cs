using System.Collections;
using UnityEngine;

public class DesktopCharacterController : MonoBehaviour
{
    [Header("Character Data")]
    public CharacterData characterData;

    [Header("Transition")]
    [Tooltip("เวลาเป็นวินาทีสำหรับเอฟเฟกต์การแปลงร่าง/สลับโหมด")]
    public float transitionDuration = 0.6f;

    public enum GameState
    {
        Tamagotchi_Idle,
        Transitioning,
        Dungeon_Active
    }

    [HideInInspector]
    public GameState currentState = GameState.Tamagotchi_Idle;

    GameObject _desktopInstance;
    GameObject _combatInstance;

    void Start()
    {
        if (characterData == null)
        {
            Debug.LogWarning("[DesktopCharacterController] CharacterData ยังไม่ถูกกำหนด โปรดลาก ScriptableObject ลงใน Inspector");
            return;
        }

        // เริ่มต้นในโหมด Tamagotchi โดย spawn ร่างจิ๋วและตั้งค่าหน้าต่าง
        EnterTamagotchi();
    }

    #region Public API (การสลับโหมด)

    public void ToggleMode()
    {
        if (currentState == GameState.Tamagotchi_Idle)
            EnterDungeon();
        else if (currentState == GameState.Dungeon_Active)
            ExitDungeon();
    }

    public void EnterDungeon()
    {
        if (currentState == GameState.Dungeon_Active || currentState == GameState.Transitioning) return;
        StartCoroutine(TransitionToDungeon());
    }

    public void ExitDungeon()
    {
        if (currentState == GameState.Tamagotchi_Idle || currentState == GameState.Transitioning) return;
        StartCoroutine(TransitionToTamagotchi());
    }

    #endregion

    #region Transitions & Spawning

    IEnumerator TransitionToDungeon()
    {
        currentState = GameState.Transitioning;

        if (_desktopInstance != null)
        {
            Destroy(_desktopInstance);
            _desktopInstance = null;
        }

        if (DesktopWindowController.Instance != null)
        {
            DesktopWindowController.Instance.ApplyDungeonWindow();
        }

        SpawnCombatPrefab();

        yield return new WaitForSeconds(transitionDuration);

        currentState = GameState.Dungeon_Active;
    }

    IEnumerator TransitionToTamagotchi()
    {
        currentState = GameState.Transitioning;

        if (_combatInstance != null)
        {
            Destroy(_combatInstance);
            _combatInstance = null;
        }

        if (DesktopWindowController.Instance != null)
        {
            DesktopWindowController.Instance.ApplyTamagotchiWindow();
        }

        SpawnDesktopPrefab();

        yield return new WaitForSeconds(transitionDuration);

        currentState = GameState.Tamagotchi_Idle;
    }

    void SpawnDesktopPrefab()
    {
        if (characterData == null || characterData.desktopPrefab == null) return;

        _desktopInstance = Instantiate(characterData.desktopPrefab, this.transform);
        _desktopInstance.transform.localPosition = Vector3.zero;
        _desktopInstance.transform.localRotation = Quaternion.identity;
        _desktopInstance.transform.localScale = Vector3.one;

        var tamago = _desktopInstance.GetComponentInChildren<MonoBehaviour>();
        // ถ้าต้องเรียกเมธอดเฉพาะของ TamagotchiBehaviour ให้แก้เป็นชนิดจริงใน prefab
    }

    void SpawnCombatPrefab()
    {
        if (characterData == null || characterData.combatPrefab == null) return;

        _combatInstance = Instantiate(characterData.combatPrefab, this.transform);
        _combatInstance.transform.localPosition = Vector3.zero;
        _combatInstance.transform.localRotation = Quaternion.identity;
        _combatInstance.transform.localScale = Vector3.one;

        var animator = _combatInstance.GetComponentInChildren<Animator>();
        if (animator != null && characterData.combatAnimatorController != null)
        {
            animator.runtimeAnimatorController = characterData.combatAnimatorController;
        }

        var combatChar = _combatInstance.GetComponentInChildren<CombatCharacter>();
        if (combatChar != null)
        {
            combatChar.Initialize(characterData.maxHP, characterData.attack, characterData.attackSpeed);
        }
    }

    void EnterTamagotchi()
    {
        if (DesktopWindowController.Instance != null)
        {
            DesktopWindowController.Instance.ApplyTamagotchiWindow();
        }

        SpawnDesktopPrefab();
        currentState = GameState.Tamagotchi_Idle;
    }

    #endregion
}

/// <summary>
/// Component เล็ก ๆ สำหรับเก็บค่าสถานะของตัวละครในโหมดต่อสู้ (วางไว้ใน combat prefab ถ้าต้องการ)
/// </summary>
public class CombatCharacter : MonoBehaviour
{
    public int hp;
    public int attack;
    public float attackSpeed;

    public void Initialize(int hpMax, int atk, float atkSpeed)
    {
        hp = hpMax;
        attack = atk;
        attackSpeed = atkSpeed;
    }
}
