`timescale 1ns / 1ps
//////////////////////////////////////////////////////////////////////////////////
// Company: 
// Engineer: 
// 
// Create Date:    23:44:06 07/28/2013 
// Design Name: 
// Module Name:    SimpleQuadratureCounter 
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
module BBot_SimpleQuadratureCounter(
    input clock,
	 input reset_l,
	 input A,
    input B,
    output [31:0] CurrentCount,
    output Direction
    );

reg BPrevious;
reg APrevious;
reg ACurrent;
reg BCurrent;
reg[31:0] Count;
reg Dir;

always @(posedge clock) begin

	if (reset_l == 1'b0)
	begin
		
		Count[31:0] <= 32'h80000000;
	
	end
	else
	begin
		
		if( (APrevious != A) || (BPrevious != B))
		begin

			//Every time A or B changes evaluate this XOR
			//If the result is 1 we count up if it's 0 we count down
			if (A ^ BPrevious)
			begin
				Count <= Count + 1'b1;
				Dir <= 1'b1;
			end
			else begin
				Count <= Count - 1'b1;
				Dir <= 1'b0;
			end
		end
		
		BPrevious <= B;
		APrevious <= A;
		
	end

end

	assign CurrentCount[31:0] = Count[31:0];
	assign Direction = Dir;
endmodule
