using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class Parede : MonoBehaviour
{
    void Update()
    {
        SetChildsIndex();

        List<ObjectController> filhos = new List<ObjectController>();

        foreach (Transform filho in transform)
        {
            ObjectController comp = filho.GetComponent<ObjectController>();
            filhos.Add(comp);
        }

        // Ordena pelo índice
        var filhosOrdenados = filhos.Where(w => !w.Visible).OrderBy(f => f.Index).ToList();

        // Valida sequência contínua a partir de 1
        bool sequenciaValida = filhosOrdenados.Count > 0;
        for (int i = 0; i < filhosOrdenados.Count; i++)
        {
            int esperado = i + 1;

            Debug.Log($"esperado:{esperado} index:{filhosOrdenados[i].Index}");

            if (filhosOrdenados[i].Index != esperado)
            {
                Debug.LogWarning($"Erro na sequência: esperava {esperado}, mas encontrou {filhosOrdenados[i].Index} no objeto {filhosOrdenados[i].name}");
                sequenciaValida = false;
                break;
            }
        }

        if (sequenciaValida)
        {
            Debug.Log("Sequência válida!");

            if (filhos.Count == filhosOrdenados.Count)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    Vector3 pos = mainCam.transform.parent.position;
                    Debug.Log($"esperado:{pos}");
                    pos.z -= 5;
                    mainCam.transform.parent.position = pos;
                }

                if (transform.parent != null)
                {
                    transform.parent.gameObject.SetActive(false);
                }

                gameObject.SetActive(false);
            }

        }
        else {
            foreach (Transform filho in transform)
            {
                ObjectController comp = filho.GetComponent<ObjectController>();
                comp.Visible = true;
                comp.gameObject.SetActive(true);
            }
        }
        
    }

    private void SetChildsIndex()
    {
        foreach (Transform filho in transform)
        {
            ObjectController comp = filho.GetComponent<ObjectController>();
            if (comp != null)
            {
                string nome = filho.name;

                if (nome.Length > 0 && char.IsDigit(nome[0]))
                {
                    comp.Index = int.Parse(nome[0].ToString());
                    // Debug.Log($"Setado Index {comp.Index} para {filho.name}");
                }
            }
        }
    }
}
