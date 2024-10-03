# CombEaredSkink
This bot was created as my entry for [Sebastian Lague](https://github.com/SebLague)'s [Chess Coding Challenge](https://youtu.be/iScy18pVR58).
The rules and details for the contest can be seen in the official repository [here](https://github.com/SebLague/Chess-Challenge).

Note that the main branch of this project contains a bunch of debugging code. For the submitted version, see the [v3](https://codeberg.org/Deepfriedice/CombEaredSkink/src/branch/v3) branch.

This bot's evaluation function doesn't care about pieces at all. Instead, it looks at the difference between the number of moves the player can make, vs the number of moves the opponent could make if it skipped its turn. This causes a bunch of interesting behaviours, and was inspired by the "min_oppt_moves" strategy in Tom 7's Elo World [video](https://www.youtube.com/watch?v=DpXy041BIlA)/[paper](http://tom7.org/chess/weak.pdf).

Search is done with Alpha-Beta Negamax, and an evaluation cache is used to reduce calls to GetLegalMoves(). Rather than implement iterative deeping, the elapsed time after each move is used to adjust the search depth.
