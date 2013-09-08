echo "Setting System Time via Real Time Clock"
echo ds1307 0x68 > /sys/class/i2c-adapter/i2c-1/new_device
hwclock -s -f /dev/rtc1
hwclock -w
echo "System Time Set"
echo "The time is:"
date
