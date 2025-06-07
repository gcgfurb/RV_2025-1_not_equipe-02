// COMEÇA A COPIAR DAQUI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla o comportamento de uma única parede de letra, gerenciando seus pontos e progressão.
/// </summary>
public class LetterWallController : MonoBehaviour
{
    [Tooltip("Lista de todos os GameObjects de ponto (Icosahedron) pertencentes a esta letra/parede.")]
    public List<GameObject> letterPoints;

    [Tooltip("Referência para o próximo LetterWallController (a próxima parede) na sequência.")]
    public LetterWallController nextWall;

    [Tooltip("Particle system a ser instanciado e reproduzido quando a parede quebra (opcional).")]
    public GameObject breakEffectPrefab; // Atribua um prefab de sistema de partículas aqui

    private int _gazedPointsCount = 0;
    private bool _wallBroken = false;

    /// <summary>
    /// Start é chamado antes do primeiro frame update.
    /// </summary>
    void Start()
    {
        // Garante que todos os pontos estejam inicialmente ativos ao carregar a parede
        foreach (GameObject point in letterPoints)
        {
            point.SetActive(true);
            // Opcional: Reseta o material do ponto para o InactiveMaterial.
            // Isso é útil se você reativar a parede em algum momento do jogo.
            ObjectController pointController = point.GetComponent<ObjectController>();
            if (pointController != null && pointController.InactiveMaterial != null)
            {
                point.GetComponent<Renderer>().material = pointController.InactiveMaterial;
            }
        }

        // No Editor Unity, desative todos os GameObjects 'model' exceto o primeiro.
        // Este script ativará o próximo 'model' quando o atual quebrar.
        if (nextWall != null)
        {
            nextWall.gameObject.SetActive(false); // Garante que as próximas paredes estejam inativas no início
        }
    }

    /// <summary>
    /// Chamado por scripts ObjectController individuais quando um ponto é olhado.
    /// </summary>
    public void PointGazedAt()
    {
        if (_wallBroken) return; // Não conta pontos se a parede já está quebrada

        _gazedPointsCount++;
        Debug.Log($"Ponto olhado em {gameObject.name}. Total olhados: {_gazedPointsCount}/{letterPoints.Count}");

        if (_gazedPointsCount >= letterPoints.Count)
        {
            BreakWall();
        }
    }

    /// <summary>
    /// Quebra a parede atual e ativa a próxima.
    /// </summary>
    private void BreakWall()
    {
        if (_wallBroken) return; // Evita quebrar a parede múltiplas vezes

        _wallBroken = true;
        Debug.Log($"{gameObject.name} está quebrando!");

        // 1. Reproduz o efeito de quebra (se houver)
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. Faz a parede desaparecer (desabilita renderers e colliders)
        // Desabilita o Renderer e Collider do GameObject 'model' principal
        Renderer wallRenderer = GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            wallRenderer.enabled = false;
        }
        Collider wallCollider = GetComponent<Collider>();
        if (wallCollider != null)
        {
            wallCollider.enabled = false;
        }

        // Desabilita os Renderers e Colliders dos filhos (TextF e outras partes do modelo da parede)
        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null) childRenderer.enabled = false;
            Collider childCollider = child.GetComponent<Collider>();
            if (childCollider != null) childCollider.enabled = false;

            // Continua para netos, se necessário (ex: Icosahedrons dentro de TextF)
            foreach (Transform grandchild in child)
            {
                Renderer grandchildRenderer = grandchild.GetComponent<Renderer>();
                if (grandchildRenderer != null) grandchildRenderer.enabled = false;
                Collider grandchildCollider = grandchild.GetComponent<Collider>();
                if (grandchildCollider != null) grandchildCollider.enabled = false;
            }
        }

        // 3. Ativa a próxima parede após um pequeno atraso
        if (nextWall != null)
        {
            StartCoroutine(ActivateNextWallAfterDelay(1.5f)); // 1.5 segundos para o efeito de quebra
        }
        else
        {
            Debug.Log("Todas as paredes concluídas! Fim de Jogo ou Nível Completo.");
            // Implemente sua lógica de fim de jogo ou transição de fase aqui.
        }
    }

    /// <summary>
    /// Corrotina para ativar a próxima parede após um atraso.
    /// </summary>
    /// <param name="delay">Tempo de atraso em segundos.</param>
    IEnumerator ActivateNextWallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        nextWall.gameObject.SetActive(true);

        // --- Lógica opcional para reposicionar o jogador/câmera ---
        // Você precisará adaptar isso à sua configuração específica de movimento do jogador.
        // Por exemplo, se seu jogador é o GameObject com a tag "Player":
        // GameObject player = GameObject.FindWithTag("Player");
        // if (player != null)
        // {
        //     // Exemplo: Mover o jogador para uma posição na frente da nova parede
        //     // e fazer o jogador olhar para a parede.
        //     Vector3 targetPosition = nextWall.transform.position - nextWall.transform.forward * 3f; // 3 unidades à frente
        //     targetPosition.y = player.transform.position.y; // Mantém a altura do jogador
        //     player.transform.position = targetPosition;
        //
        //     // Faz o jogador olhar para o centro da nova parede
        //     player.transform.LookAt(nextWall.transform.position);
        // }
        // -------------------------------------------------------------
    }

    // --- Lógica opcional para reiniciar a parede ---
    // Este método é útil se você tiver um sistema de "tentar novamente"
    // ou se quiser reiniciar um nível após um game over.
    // Ele não é chamado automaticamente por este código principal,
    // mas você pode chamá-lo de um GameManager ou botão de UI.
    public void ResetWall()
    {
        _gazedPointsCount = 0;
        _wallBroken = false;

        // Reabilita renderers e colliders da parede principal e seus filhos
        Renderer wallRenderer = GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            wallRenderer.enabled = true;
        }
        Collider wallCollider = GetComponent<Collider>();
        if (wallCollider != null)
        {
            wallCollider.enabled = true;
        }

        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null) childRenderer.enabled = true;
            Collider childCollider = child.GetComponent<Collider>();
            if (childCollider != null) childCollider.enabled = true;

            foreach (Transform grandchild in child)
            {
                Renderer grandchildRenderer = grandchild.GetComponent<Renderer>();
                if (grandchildRenderer != null) grandchildRenderer.enabled = true;
                Collider grandchildCollider = grandchild.GetComponent<Collider>();
                if (grandchildCollider != null) grandchildCollider.enabled = true;
            }
        }

        // Reativa e reseta os pontos para o estado inicial
        foreach (GameObject point in letterPoints)
        {
            point.SetActive(true);
            ObjectController pointController = point.GetComponent<ObjectController>();
            if (pointController != null && pointController.InactiveMaterial != null)
            {
                point.GetComponent<Renderer>().material = pointController.InactiveMaterial;
            }
        }

        // Garante que a próxima parede esteja inativa, a menos que seja explicitamente ativada.
        if (nextWall != null)
        {
            nextWall.gameObject.SetActive(false);
        }
    }
    // -------------------------------------------------------------
}
// TERMINA A COPIA AQUI