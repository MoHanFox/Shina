using UnityEngine;

public class CharacterAnimationManger: MonoBehaviour
{
    public Audiobook audiobook;
    
    public void Apply(string value)
    {
        //如:name A B C D ...
        var 参数 = value.Split(" ");

        var name = 参数[0];

        if (name == "A_MovedOver_Hit_B")
        {
            var animation = gameObject.AddComponent<A_MovedOver_Hit_B>();
            animation.A = gameObject.transform.GetChild(int.Parse(参数[1]));
            animation.B = gameObject.transform.GetChild(int.Parse(参数[2]));
        }
    }
    
    public void disEmploy()
    {
        if(!hasAnimation())
            return;
        
        foreach (var VARIABLE in gameObject.GetComponents<CharacterAnimation>())
        {
            Destroy(VARIABLE);
        }
        
        audiobook.恢复立绘位置();
    }
    
    public bool hasAnimation()
    {
        return gameObject.GetComponent(typeof(CharacterAnimation)) != null;
    }
}