#shell scp ~/_BBot_Linux/Source Code/BBot_Embedded/Arm BB Toolchain/BBot_Embedded root@192.168.1.49:/home/lasx
#shell ssh root@192.168.1.49 gdbserver :12345 ~/BBot_Embedded &
target remote 192.168.1.49:12345