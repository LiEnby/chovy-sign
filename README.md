#ChovyProject 
Created by the CBPS Team!
https://discord.gg/2nDCbxJ

- chovy-sign is an application to allow you to convert PSP ISO's to be playable on unmodified PSVita's

Setups:
You need a "base" psp game, petz saddle club, locoroco midnight carnival, or ape quest starter pack all work
some games dont work properly (where working on it). just try another if this happens.

as well as "the clone issue" basically games made with this are "clones" of the original "base game" so if u delete any of them it acturlaly deletes the license.rif which breaks all the other "clones", this can be fixed by re-downloading the base game off of the PSN or copying any "clone" over to your psvita again.

create playable PSP ISO bubbles on your PSVita

had to go public early due to qwikrazor.. thank him later..

PSP PIRACY on 3.73  
(though, WE DONT CONDONE PIRACY plz ONLY use it for games u acturaly own the UMD for) 
its purely for BACKUPS xD
also, lots of shittily dumped iso's online, however iso's obtained by the NPS Browser should work fine.
if you want to dump your own ISO's use UMD Dumper  or something that isnt ftp-ing over all the files and building it into an ISO with UMDgen or some other tool

Credits:    
dots-tb (for [chovy-gen](https://github.com/dots-tb/chovy-gen) (\_\_sce_ebootpbp signing)         
SilicaAndPina: DLL fork of [chovy-gen](https://github.com/KuromeSan/chovy-gen)
               Developing GUI, finding the psp bubbles method and [psvimgtools](https://github.com/yifanlu/psvimgtools) .NET port  
               [Sfo.NET](https://github.com/KuromeSan/Sfo.NET/blob/master/README.md) SFO Parser Code..
Motoharu (For helping dots with \_\_sce_ebootpbp)                 
xXxTheDarkprogramerxXx (For [PSPTools](https://github.com/xXxTheDarkprogramerxXx/PSPTools))               
RupertAvery (For [PSXPackager](https://github.com/RupertAvery/PSXPackager) and his fork of DiscUtils)                  
Dark_Alex (For [POPStation](https://aur.archlinux.org/packages/popstation_md/))                  
swarzesherz (For [sign_np](https://github.com/swarzesherz/sign_np))             
tpunix (For [kirk_engine](https://github.com/tpunix/kirk_engine))             
yifanlu & xyz for [psvimgtools](https://github.com/yifanlu/psvimgtools). (Chovy-Sign beta02 and lower)           
Mathieulh (Found psp signing keys?)            
MobyGames ([mobygames.com](https://www.mobygames.com/) for PS1 Cover Art)              

# What are the files
  +   CHOVY-GEN is a fork of dots_tb's [chovy-gen](https://github.com/dots-tb/chovy-gen) to MSVC as a DLL
  +   CHOVY-KIRK parts of [kirk_engine](https://github.com/tpunix/kirk_engine) and [sign_np](https://github.com/swarzesherz/sign_np) code. ported to MSVC as a DLL.
  +   CHOVY-SIGN Main GUI code, also statically includes some code from [PSXPackager](https://github.com/RupertAvery/PSXPackager) for POPS.
      It also handles .psvimg / psvmd creation, CMA Key derivation and SFO Parsing (edited (Sfo.NET)[https://github.com/KuromeSan/Sfo.NET]
      It also contains a fork of DiscUtils to add support for reading PS1 Disc Images.
# Building
In order to build CHOVY-SIGN.DLL you need [OpenSSL](https://www.npcglib.org/~stathis/downloads/openssl-1.1.0f-vs2017.7z) libary    
Extract to C:/OpenSSL or change linking settings in the solution to match your install location   
Open the SLN in Viual Studio 2019 and press "Build Solution"    
