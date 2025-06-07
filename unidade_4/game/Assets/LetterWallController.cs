// COME�A A COPIAR DAQUI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla o comportamento de uma �nica parede de letra, gerenciando seus pontos e progress�o.
/// </summary>
public class LetterWallController : MonoBehaviour
{
    [Tooltip("Lista de todos os GameObjects de ponto (Icosahedron) pertencentes a esta letra/parede.")]
    public List<GameObject> letterPoints;

    [Tooltip("Refer�ncia para o pr�ximo LetterWallController (a pr�xima parede) na sequ�ncia.")]
    public LetterWallController nextWall;

    [Tooltip("Particle system a ser instanciado e reproduzido quando a parede quebra (opcional).")]
    public GameObject breakEffectPrefab; // Atribua um prefab de sistema de part�culas aqui

    private int _gazedPointsCount = 0;
    private bool _wallBroken = false;

    /// <summary>
    /// Start � chamado antes do primeiro frame update.
    /// </summary>
    void Start()
    {
        // Garante que todos os pontos estejam inicialmente ativos ao carregar a parede
        foreach (GameObject point in letterPoints)
        {
            point.SetActive(true);
            // Opcional: Reseta o material do ponto para o InactiveMaterial.
            // Isso � �til se voc� reativar a parede em algum momento do jogo.
            ObjectController pointController = point.GetComponent<ObjectController>();
            if (pointController != null && pointController.InactiveMaterial != null)
            {
                point.GetComponent<Renderer>().material = pointController.InactiveMaterial;
            }
        }

        // No Editor Unity, desative todos os GameObjects 'model' exceto o primeiro.
        // Este script ativar� o pr�ximo 'model' quando o atual quebrar.
        if (nextWall != null)
        {
            nextWall.gameObject.SetActive(false); // Garante que as pr�ximas paredes estejam inativas no in�cio
        }
    }

    /// <summary>
    /// Chamado por scripts ObjectController individuais quando um ponto � olhado.
    /// </summary>
    public void PointGazedAt()
    {
        if (_wallBroken) return; // N�o conta pontos se a parede j� est� quebrada

        _gazedPointsCount++;
        Debug.Log($"Ponto olhado em {gameObject.name}. Total olhados: {_gazedPointsCount}/{letterPoints.Count}");

        if (_gazedPointsCount >= letterPoints.Count)
        {
            BreakWall();
        }
    }

    /// <summary>
    /// Quebra a parede atual e ativa a pr�xima.
    /// </summary>
    private void BreakWall()
    {
        if (_wallBroken) return; // Evita quebrar a parede m�ltiplas vezes

        _wallBroken = true;
        Debug.Log($"{gameObject.name} est� quebrando!");

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

            // Continua para netos, se necess�rio (ex: Icosahedrons dentro de TextF)
            foreach (Transform grandchild in child)
            {
                Renderer grandchildRenderer = grandchild.GetComponent<Renderer>();
                if (grandchildRenderer != null) grandchildRenderer.enabled = false;
                Collider grandchildCollider = grandchild.GetComponent<Collider>();
                if (grandchildCollider != null) grandchildCollider.enabled = false;
            }
        }

        // 3. Ativa a pr�xima parede ap�s um pequeno atraso
        if (nextWall != null)
        {
            StartCoroutine(ActivateNextWallAfterDelay(1.5f)); // 1.5 segundos para o efeito de quebra
        }
        else
        {
            Debug.Log("Todas as paredes conclu�das! Fim de Jogo ou N�vel Completo.");
            // Implemente sua l�gica de fim de jogo ou transi��o de fase aqui.
        }
    }

    /// <summary>
    /// Corrotina para ativar a pr�xima parede ap�s um atraso.
    /// </summary>
    /// <param name="delay">Tempo de atraso em segundos.</param>
    IEnumerator ActivateNextWallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        nextWall.gameObject.SetActive(true);

        // --- L�gica opcional para reposicionar o jogador/c�mera ---
        // Voc� precisar� adaptar isso � sua configura��o espec�fica de movimento do jogador.
        // Por exemplo, se seu jogador � o GameObject com a tag "Player":
        // GameObject player = GameObject.FindWithTag("Player");
        // if (player != null)
        // {
        //     // Exemplo: Mover o jogador para uma posi��o na frente da nova parede
        //     // e fazer o jogador olhar para a parede.
        //     Vector3 targetPosition = nextWall.transform.position - nextWall.transform.forward * 3f; // 3 unidades � frente
        //     targetPosition.y = player.transform.position.y; // Mant�m a altura do jogador
        //     player.transform.position = targetPosition;
        //
        //     // Faz o jogador olhar para o centro da nova parede
        //     player.transform.LookAt(nextWall.transform.position);
        // }
        // -------------------------------------------------------------
    }

    // --- L�gica opcional para reiniciar a parede ---
    // Este m�todo � �til se voc� tiver um sistema de "tentar novamente"
    // ou se quiser reiniciar um n�vel ap�s um game over.
    // Ele n�o � chamado automaticamente por este c�digo principal,
    // mas voc� pode cham�-lo de um GameManager ou bot�o de UI.
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

        // Garante que a pr�xima parede esteja inativa, a menos que seja explicitamente ativada.
        if (nextWall != null)
        {
            nextWall.gameObject.SetActive(false);
        }
    }
    // -------------------------------------------------------------
}
// TERMINA A COPIA AQUI