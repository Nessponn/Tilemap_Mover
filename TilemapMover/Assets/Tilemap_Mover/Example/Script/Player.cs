using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rbody;
    public float speed = 4;
    public float jumppower = 5;

    private float x;
    private bool jump;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            x = transform.position.x;
            rbody.velocity = new Vector2(-speed, rbody.velocity.y);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            x = transform.position.x;
            rbody.velocity = new Vector2(speed, rbody.velocity.y);
        }
        else
        {
            if(jump)transform.position = new Vector2(x, transform.position.y);
            else
            {
                x = transform.position.x;
            }
        }

        if (jump)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                rbody.velocity = new Vector2(rbody.velocity.x, jumppower);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //レイヤー判定の方が着地判定が非常に正確
        string layer = LayerMask.LayerToName(col.gameObject.layer);
        if(layer == "floor")
        {
            jump = true;
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        string layer = LayerMask.LayerToName(col.gameObject.layer);
        if (layer == "floor")
        {
            jump = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        string layer = LayerMask.LayerToName(col.gameObject.layer);
        if (layer == "floor")
        {
            jump = false;
        }
    }
}
