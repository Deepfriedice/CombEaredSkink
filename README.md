# CombEaredSkink
This bot was created as my entry for [Sebastian Lague](https://github.com/SebLague)'s [Chess Coding Challenge](https://youtu.be/iScy18pVR58).
The rules and details for the contest can be seen [here](https://github.com/SebLague/Chess-Challenge).

This bot's evaluation function doesn't care about pieces at all. Instead, it looks at the difference between the number of moves the player can make, vs the number of moves the opponent could make if it skipped its turn. This causes a bunch of interesting behaviours.

Search is done with Alpha-Beta Negamax, and an evaluation cache is used to reduce calls to GetLegalMoves(). Rather than implement iterative deeping, the elapsed time after each move is used to adjust the search depth.
