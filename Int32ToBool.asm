; aim - with a 32-bit integer type, turn zero into zero (false) and non-zero into one (true)
; compiled with 64 bit VS command prompt using: 
; 'ml64 Int32ToBool.asm /link /subsystem:windows /defaultlib:kernel32.lib /defaultlib:msvcrt.lib /DLL /EXPORT:one /EXPORT:two /EXPORT:three /EXPORT:four'

.code

; idea one
PUBLIC one
one PROC ; ecx:int32 -> eax:int32
    xor eax, eax ; <--- these 2 can be executed simultaneously - on intel, the xor is eliminated at rename, but apparently not on AMD
    test ecx, ecx ; <-/
    setnz al ; <-- this has a dependency on both the 'xor' and the 'test'
    ret
one ENDP

; these next 3 are the basic same pattern of test setcc movzx, but differ in what register they setcc before zero extending
; the idea is that movzx eax, al isn't ideal because supposedly on Haswell + Skylake a movzx <reg> <reg> can be eliminated by the renamer,
; but not when both registers are part of the same reg (eax and al are subsections of rax). So instead, we setcc and then extend it to eax,
; so this should be eliminated during rename - this does however mean we now have a dependency on test to finish (as it uses ecx), but I don't think
; it should be an issue because we already had the dependency on the flags set by test. If it is an issue, idea 4 instead uses the unused dl register for
; setcc

; all the 'setnz <l8>' ops have a dependency on the 'test'

; idea two
PUBLIC two
two PROC ; ecx:int32 -> eax:int32
    test ecx, ecx 
    setnz al ; this has a dependency on the flags from 'test'
    movzx eax, al ; <-- this has a dependency  on 'setcc' which in turn has the dependency on 'test' - however, apparently on 
                  ; *some* (not all) newer chips it can be rename eliminated - but maybe not when it has that dependency?
    ret
two ENDP

; idea three
PUBLIC three
three PROC ; ecx:int32 -> eax:int32
    test ecx, ecx
    setnz cl ; this has a dependency on the flags AND register from 'test', because it writes to eax/cl
    movzx eax, cl 
    ret
three ENDP

; idea four
PUBLIC four
four PROC ; ecx:int32 -> eax:int32
    test ecx, ecx
    setnz dl ; this has a dependency on the flags from 'test' but not the register (like two:) but it still has the different registers for the 'movzx'
    movzx eax, dl
    ret
four ENDP

End