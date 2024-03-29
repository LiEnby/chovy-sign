#ChovyProject v2
Now with PS1 Support !

**chovy-sign is an application to allow you to convert PSP and PS1 ISO's to be playable on unmodified PSVita***

Setups:
You need an (offical) PSP or PS1 license;

i recommend getting; Petz Saddle Club, LocoRoco Midnight Carnival, or Ape Quest Starter Pack
because atleast one of these are free in *most* regions; 


----
**-- If you don't have a modded PSVita:**

*you will need:*
- Any PS1 or PSP game (or demo) from the PS Store. (Petz Saddle Club, LocoRoco Midnight Carnival, etc..)
- Ability to connect PSVita to PC Content Manager (USB or WiFi)

*obtaining keys:*
- Copy an offical PS1 or PSP game to your PC using Content Manager,
- open chovy-sign2
- select either PSP or PS1 and click the "Get Keys" button
- click "EBOOT.PBP method", 
- select the offical PS1 or PSP game you copied;
- click on select "Content Manager" backup

- you should see the RIF and KEY fields populate,

*chovy-sign2 will remember this information for later so you only have to do this part one time;*

*getting psp or ps1 games:*
- select either a PS1 BIN/CUE image or a UMD ISO image.
- click "go"
- once it finishes, you should be able to find the game availible to be copied over from Content Manager.

note: *there may be some issues if you try this while having DLC for the game installed .*

----
**-- If you have a modded PSVita:**

note: *you can still use the unmodified vita method on a hacked vita; if you would like *

note2: *bubbles created using the hacked vita method will still *work* on any unmodified vita, *
*using the same PSN Account; however it requires a hacked vita to *obtain* some of the files, *
*that are required for this method;*

*you will need:*
- Your ConsoleID/IDPS, Yoti has a tool to dump this availible [here](https://github.com/Yoti/psv_idpsdump/releases/)
- Your Consoles activation data file (located at ``tm0:/npdrm/act.dat``)
- Any offical PSP/PS1 License file for your account (located at ``ux0:/pspemu/PSP/LICENSE/*.rif``, or ``ux0:/bgdl/t/XXXXXX/temp.dat`` for downloads) 
*you can also use PocketStation license or a PSP DLC license*

note: *your IDPS is unique to your PSVita and is used to identify your console on the PSN, so do not share it with other people*

*obtaining keys:*
- open chovy-sign2
- select either PSP or PS1 and click the "Get Keys" button
- select "IDPS+RIF+ACT Method",
- copy in your IDPS
- browse for your license rif file
- browse for our act.dat file
- click generate keys

*chovy-sign2 will remember this information for later so you only have to do this part one time;*

*getting psp or ps1 games:*
- select either a PS1 BIN/CUE image or a UMD ISO image.
- click "go"
- once it finishes, you should be able to find the game availible to be copied over from Content Manager.
- alternatively, you can disable the "Use CMA" option in settings, and then copy the resulting **TITLEID** folder to ``ux0:/pspemu/PSP/GAME``, then delete ``ux0:/id.dat``, and reboot to trigger an app database update.

-----

so overall there are 3 methods to obtain keys,
![image](https://silica.codes/SilicaAndPina/chovy-sign/raw/branch/master/Methods.png)

note: *keys.txt is basically pointless now, in the bast it wasn't known how to use psp eboot to create ps1 games*

----

Credits:    
SquallATF (for "PspCrypto" and "PBPResigner", and making PS1 Game signing possible,
And discovering a bug in \_\_sce_ebootpbp handling that makes it possible to use multi-disc games.)

Li: (for writing the original chovy-sign,
Developing the GUI, finding the original psp bubbles method and,
[Sfo.NET](https://github.com/KuromeSan/Sfo.NET/blob/master/README.md) A Simple SFO Parser,             
Writing the PS1 Disc Compresison algorithm, making it possible to use custom ISOs,
Writing C# Implementation of PSVIMGTOOLS,
Being transgender)
				
dots-tb (for [chovy-gen](https://github.com/dots-tb/chovy-gen) (\_\_sce_ebootpbp signing)         

Sony Computer Enterainment: (For at3tool)

yifanlu & xyz for the original [psvimgtools](https://github.com/yifanlu/psvimgtools). 

Mathieulh (Found psp signing keys?)            

Motoharu (For helping dots with \_\_sce_ebootpbp)                 
Davee (For finding the PS1 Signing Key)
xlenore (For [PS1 Cover Art](https://github.com/xlenore/psx-covers))              
RupertAvery (For their fork of DiscUtils to add PS1 Support)  

SquallATF's PBPResigner and PSPCrypto were derived from :

xdotnano ([PSXTract](https://github.com/xdotnano/PSXtract))             

swarzesherz ([sign_np](https://github.com/swarzesherz/sign_np))             

tpunix ([kirk_engine](https://github.com/tpunix/kirk_engine))             
