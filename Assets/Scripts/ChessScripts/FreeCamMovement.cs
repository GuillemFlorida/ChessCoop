using UnityEngine;

public class FreeCamController : MonoBehaviour
{
    public float velocidad = 10f;
    public float sensibilidad = 3f;

    void Start()
    {
        
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal"); 
        float z = Input.GetAxisRaw("Vertical");  

        float y = 0f;
        if (Input.GetKey(KeyCode.E)) y = 1f;  
        if (Input.GetKey(KeyCode.Q)) y = -1f; 

        Vector3 movimiento = new Vector3(x, y, z);
        transform.Translate(movimiento * velocidad * Time.deltaTime);


        if (Input.GetMouseButton(1))
        {
            float ratonX = Input.GetAxis("Mouse X") * sensibilidad;
            float ratonY = Input.GetAxis("Mouse Y") * sensibilidad;

            transform.Rotate(Vector3.up, ratonX, Space.World);   
            transform.Rotate(Vector3.right, -ratonY, Space.Self); 
        }
    }

    void LateUpdate()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}