#!/bin/bash

#OMAP - mux reg is defined as
# 10  9  8  7  6  5  4  3  2  1  0
# 0 1 and 2 are the mux mode 0 though 7
# 3 is pullup/pulldown enable where 0 is enable and 1 is disable
# 4 is pull type select where 0 is pulldown and 1 is pullup
# 5 "receiver enable" where 0 is receiver disabled and 1 is enabled
#    this essentially sets it as in input or output
# 6 slew rate where 0 is fast and 1 is slow

echo "Bash: Applying BBot Overlay"
SLOTS="/sys/devices/bone_capemgr.8/slots"
echo "Bash: Moving BBot Overlay File"
rm /lib/firmware/BBot-Overlay-00A0.dtbo
cp /home/root/BBot-Overlay-00A0.dtbo /lib/firmware
echo "Bash: Applying BBot DeviceTree Overlay"
echo BBot-Overlay > $SLOTS
echo "Bash: Deleting current text.txt log file"
rm /home/root/text.txt 
echo "Bash: Done Applying BBot Overlay"
