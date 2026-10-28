using UnityEngine;

public class Health : MonoBehaviour
{
    public int vida = 100;   

    public void QuitarVida(int dano)
    {
        vida = vida - dano;
        Debug.Log("Has hecho daño a " + gameObject.name + ", le quedan " + vida + " de vida");

        if (vida <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Debug.Log(gameObject.name + " ha muerto");
        Destroy(gameObject);   
}
}