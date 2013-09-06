#!/bin/bash

#OMAP - mux reg is defined as
# 10  9  8  7  6  5  4  3  2  1  0
# 0 1 and 2 are the mux mode 0 though 7
# 3 is pullup/pulldown enable where 0 is enable and 1 is disable
# 4 is pull type select where 0 is pulldown and 1 is pullup
# 5 "receiver enable" where 0 is receiver disabled and 1 is enabled
#    this essentially sets it as in input or output
# 6 slew rate where 0 is fast and 1 is slow

clear
echo "Reading UART1 Pins settings"
cat /sys/kernel/debug/omap_mux/uart1_rxd
cat /sys/kernel/debug/omap_mux/uart1_txd
echo "Setting up UART1 Pins"
echo 20 > /sys/kernel/debug/omap_mux/uart1_rxd
echo 0 > /sys/kernel/debug/omap_mux/uart1_txd
echo "Reading new UART1 Settings"
cat /sys/kernel/debug/omap_mux/uart1_rxd
cat /sys/kernel/debug/omap_mux/uart1_txd

echo "Reading UART2 Pins Settings"
cat /sys/kernel/debug/omap_mux/spi0_d0		#tx
cat /sys/kernel/debug/omap_mux/spi0_sclk	#rx
echo "Setting up UART2 Pins"
echo 1 > /sys/kernel/debug/omap_mux/spi0_d0
echo 21 > /sys/kernel/debug/omap_mux/spi0_sclk
echo "Reading new UART2 Pin Settings"
cat /sys/kernel/debug/omap_mux/spi0_d0
cat /sys/kernel/debug/omap_mux/spi0_sclk

echo "Reading GPIO1_6 (GPMC_AD6)"
cat /sys/kernel/debug/omap_mux/gpmc_ad6
echo 0 > /sys/kernel/debug/omap_mux/gpmc_ad6 #set as output
echo "Reading GPIO1_7 (GPMC_AD2)"
cat /sys/kernel/debug/omap_mux/gpmc_ad2
echo 0 > /sys/kernel/debug/omap_mux/gpmc_ad2 #set as output
echo "Reading GPIO1_13 (GPMC_AD13)"
cat /sys/kernel/debug/omap_mux/gpmc_ad13
echo 0 > /sys/kernel/debug/omap_mux/gpmc_ad13 #set as output

