using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityVitals : MonoBehaviour
{
    private enum RegenModes
    {
        noNaturalRegen, //Does not naturally regenerate (requires a spell / potion / something else to regenerate)
        requiresMagica, //Requires magica to regenerate
        requiresHealth, //requires health to regenerate
        requiresStamina,//requires stamina to regenerate
        requiresTimeSinceLastUse //Requires resource to not be drained for x time to regenerate
    }

    [Header("Health")]
    [SerializeField] private float maxHealth;
    [SerializeField] private RegenModes healthRegenType; //What method of regen health should use
    [SerializeField] private float healthRegenSpeed; //Per burst of regen, how much health to restore
    [SerializeField] private float healthRegenRequireAmmount; //Per unit of health, how much "required" resource (seconds if time, units if other)"
    [SerializeField] private float healthRegenDelay; //How long between burst of regen
    private float currentHealth;

    [Header("Stamina")]
    [SerializeField] private float maxStamina;
    [SerializeField] private RegenModes staminaRegenType;
    [SerializeField] private float staminaRegenSpeed; //Per burst of regen, how much stam to restore
    [SerializeField] private float staminaRegenRequireAmmount;
    [SerializeField] private float staminaRegenDelay;
    private float currentStamina;

    [Header("Magica")]
    [SerializeField] private float maxMagica;
    [SerializeField] private RegenModes magicaRegenType;
    [SerializeField] private float magicaRegenSpeed;
    [SerializeField] private float magicaRegenRequireAmmount;
    [SerializeField] private float magicaRegenDelay;
    private float currentMagica;

    #region private methods

    //Set up resistances



    #endregion

    #region public getters / setters / changers
    #region Health
    public void SetHealth(float newHealth)
    {
        currentHealth = newHealth;
    }

    public float GetHealth()
    {
        return currentHealth;
    }
    
    public void ChangeHealth(float changeAmount)
    {
        currentHealth += changeAmount;
    }
    #endregion

    #region stamina
    public void SetStamina(float newStamina)
    {
        currentStamina = newStamina;
    }

    public float GetStamina()
    {
        return currentStamina;
    }

    public void ChangeStamina(float changeAmount)
    {
        currentStamina += changeAmount;
    }
    #endregion

    #region magica
    public void SetMagica(float newMagica)
    {
        currentMagica = newMagica;
    }

    public float GetMagica()
    {
        return currentMagica;
    }

    public void ChangeMagica(float changeAmount)
    {
        currentMagica += changeAmount;
    }
    #endregion
    #endregion

}
