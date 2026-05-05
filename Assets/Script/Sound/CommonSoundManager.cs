using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//SEÇÃñºëOê›íË
public enum CommonSoundType
{
    Dodge,StartButton,BattleStart,NextButton,TitleBackButton,Buy,Sell,Click,WaveClear,ItemSet,NormalButton,Heal,Buff,GameOver,TakeDamage,Beep,
    Guard,Hold,
}

public class CommonSoundManager : MastarSoundManager<CommonSoundType>
{

}
