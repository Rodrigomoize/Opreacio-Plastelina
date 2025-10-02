using System.Collections.Generic;
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

    private NavMeshAgent agent;
    private bool isInitialized = false;
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

    public void SetupFromCard(CardManager.Card card, Team spawnTeam = Team.Player)
    {
        if (card == null)
        {
            Debug.LogError("SetupFromCard recibi� card null");
            return;
        }

        charName = card.cardName;
        maxLife = card.cardLife;
        currentLife = maxLife;
        velocity = card.cardVelocity;
        value = card.cardValue;
        Characters = card.fbxCharacter;
        team = spawnTeam;

        isInitialized = true;
        OnSpawned();
    }

    private void OnSpawned()
    {
        // c�digo de inicio: buscar objetivo, animaciones, etc.
        // Por ejemplo, buscar la torre enemiga por tag y moverse hacia ella:
        GameObject target = GameObject.FindWithTag(team == Team.Player ? "EnemyTower" : "PlayerTower");
        if (target != null && agent != null)
        {
            agent.SetDestination(target.transform.position);
        }
    }

    // Ejemplo de da�o
    public void TakeDamage(int value)
    {
        currentLife -= value;
        if (currentLife <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // efectos, liberaci�n de recursos, notificar CharacterManager si necesitas
        Destroy(gameObject);
    }

    // Ejemplo de colisi�n / trigger para resoluci�n ataque/defensa
    // Asume que ambos characters tienen este script y un Collider con isTrigger = true
    private void OnTriggerEnter(Collider other)
    {
        Character otherChar = other.GetComponent<Character>();
        if (otherChar == null) return;

        // evitar friendly fire
        if (otherChar.team == this.team) return;

        // Resoluci�n simplificada:
        // Si este character es defensor (p.ej. single card) y value == otherChar.value (operaci�n)
        // gana el defensor (destroy attacker), de lo contrario no pasa nada (seg�n tu dise�o).
        bool thisIsDefender = IsDefender(); // define seg�n tu l�gica (por ejemplo, isCombined flag)
        bool otherIsDefender = otherChar.IsDefender();

        // En tu juego: attacker vs defender. Define qu� roles tienen.
        // Ej: si one is defender and values equal -> defender kills attacker
        if (!thisIsDefender && otherIsDefender)
        {
            // other is defender, this is attacker
            if (otherChar.value == this.value)
            {
                // defender wins
                this.Die();
                intelectManager.AddIntelect(1);
                // maybe grant intellect to defender's owner
            }
            else
            {
                // attacker continues - or implement alternate logic
                //pensar en meter un camerashake o resta de puntos 
            }
        }
        else if (thisIsDefender && !otherIsDefender)
        {
            if (this.value == otherChar.value)
            {
                otherChar.Die(); // defender kills attacker
            }
        }
        else
        {
            // two attackers or two defenders: default behaviour (e.g., ignore or fight)
        }
    }

    // Define como averiguar si este character es defensor o atacante
    public bool IsDefender()
    {
        // Esto depende de c�mo marcas las cartas. Si tus defense-cards tienen isCombined = false, por ejemplo.
        // Aqu� asumimos que value <= 5 (singles) son defenders, y operations tienen algun flag.
        // Mejor: a�ade un bool en Card: isAttack / isDefense y p�salo en SetupFromCard.
        return false; // placeholder: reemplaza por tu criterio
    }
}

