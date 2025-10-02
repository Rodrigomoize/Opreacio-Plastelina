using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;

public class Character : MonoBehaviour
{

    public string charName;
    public int maxLife ;
    public int currentLife ;
    public int velocity ;
    public int value;
    public GameObject Characters;


    CardManager cardManager;
    IntelectManager intelectManager;
    CombinedCharacter parentCombined;

    private NavMeshAgent agent;
    private bool isInitialized = false;

    private int operationResult = int.MinValue; // si == int.MinValue -> no aplica
    private char opSymbol;



    public enum Team { Player, Enemy }
    public Team team = Team.Player;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupFromCard(CardManager.Card card, Character.Team t, IntelectManager intelect, int operationResult = int.MinValue, char opSymbol = '\0')
    {
        charName = card.cardName;
        maxLife = card.cardLife;
        currentLife = maxLife;
        value = card.cardValue;
        team = t;
        this.operationResult = operationResult;
        this.opSymbol = opSymbol;
        intelectManager = intelect;
        parentCombined = GetComponentInParent<CombinedCharacter>();
    }

    private void OnSpawned()
    {
        // código de inicio: buscar objetivo, animaciones, etc.
        // Por ejemplo, buscar la torre enemiga por tag y moverse hacia ella:
        GameObject target = GameObject.FindWithTag(team == Team.Player ? "EnemyTower" : "PlayerTower");
        if (target != null && agent != null)
        {
            agent.SetDestination(target.transform.position);
        }
    }

    private void Die()
    {
       
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si colisiono con una defensa single
        Character defender = other.GetComponent<Character>();
        if (defender != null && defender.team != this.team)
        {
            bool defenderIsSingle = defender.IsDefender(); // Implementa IsDefender en Character basado en cardType
            if (!defenderIsSingle) return;

            // Si esta unidad forma parte de un ataque combinado -> usar operationResult para comparar
            if (operationResult != int.MinValue)
            {
                if (defender.value == operationResult)
                {
                    // Defender gana -> destruir atacante unit
                    Die();
                    // defender no muere, no sumar intelecto
                }
                else
                {
                    // Defender muere -> sumar intelecto
                    defender.Die();
                    if (intelectManager != null) intelectManager.AddIntelect(1);
                }
            }
            else
            {
                // Si no hay operationResult (unidad simple atacante), comparar defender vs this.value o aplica otra lógica
            }
        }
    }

    // Define como averiguar si este character es defensor o atacante
    public bool IsDefender()
    {
        return false; // placeholder: reemplaza por tu criterio
    }
}

