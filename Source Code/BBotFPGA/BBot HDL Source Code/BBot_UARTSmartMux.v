`timescale 1ns / 1ps
//////////////////////////////////////////////////////////////////////////////////
// Company: 
// Engineer: 
// 
// Create Date:    23:45:24 07/28/2013 
// Design Name: 
// Module Name:    UARTDemux 
// Project Name: 
// Target Devices: 
// Tool versions: 
// Description: 
//
// Dependencies: 
//
// Revision: 
// Revision 0.01 - File Created
// Additional Comments: 
//
//////////////////////////////////////////////////////////////////////////////////
//This is a 1 to 4 demux circuit of data width = 1
//The TX and RX lanes will each have one of these module instances
module BBot_UartSmartMux(
	input clock,
	input reset_l,
	input [2:0] selectRoute,
	input [N-1:0] tx3_in, tx2_in, tx1_in, tx0_in,
	output reg [N-1:0] rx3_out, rx2_out, rx1_out, rx0_out
);

//Signal set 0 is XBee
//Signal set 1 is BBone
//Signal set 2 is Text to Speach Module
//Signal set 3 is undefined

//Set Width to one - this could be set to any width but 
//we we only have 1 wire for TX signals (in) and 1 wire for RX signals (out)
//Code demonstrates a configurable data width for the inputs and outputs of the 
//module but it's not really needed here...
parameter N = 1;

always @(posedge clock) begin
	
	if (reset_l == 1'b0)
	begin
		rx3_out <= {N{1'b0}};
		rx2_out <= {N{1'b0}};
		rx1_out <= {N{1'b0}};
		rx0_out <= {N{1'b0}};
		
	end
	else
	begin
		
		//Not using the 3'rd bit yet... use X for "don't care"
		if (selectRoute == 3'bX11) 
			begin
				//Not defined
			
			end
		if (selectRoute == 3'bX10) 
			begin
				//Not defined
		
			end
		if (selectRoute == 3'bX01) 
			begin
				//BBone is TX maser, voice module is slave (receiver)
				rx2_out <= tx1_in;
				rx1_out <= tx2_in;
			end
			else
			begin
				rx3_out <= {N{1'b0}};
				rx0_out <= {N{1'b0}};
			end
		if (selectRoute == 3'bX00) 
			begin
				//XBee signals are TX master, BBone is slave (RX receiver)
				rx1_out <= tx0_in;
				rx0_out <= tx1_in;
			end
			else
			begin
				//All other output nets not involved are asserted to 0
				rx3_out <= {N{1'b0}};
				rx2_out <= {N{1'b0}};
			end
		end
end
		
endmodule		