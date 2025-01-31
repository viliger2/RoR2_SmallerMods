# RevertStunBuff
Reverts stun buffs to how they were pre-SoTS. It can probably be run server-side only, but I haven't tested it.

# Background stuff
Basically, pre-SoTS stuns would refresh their duration. With SoTS Gearbox decided that stuns instead should stack. 

![image](https://raw.githubusercontent.com/viliger2/RoR2_SmallerMods/refs/heads/main/RevertStunBuff/Thunderstore/code_example.png)

As a result something like Suppressive Fire results in absurdly long stun compared to how it was before. Notice that stun visual effect ends way before actual stun state runs out. In this example, Golem is stunned for 6.5 seconds instead of around 1.5 pre-SoTS.

![image](https://raw.githubusercontent.com/viliger2/RoR2_SmallerMods/refs/heads/main/RevertStunBuff/Thunderstore/example.gif)