You can find a compiled windows build in the "Builds" folder.

CONTROLS

	Left Click: Spawn a "Game of Life" cell, and press buttons.
	Right Click: Spawn a cell with the rule that appears in the "Rightclick" button.
	
BUTTONS
	
	Reset: Resets the field, clearing all the cells.
	Rightclick: Not to be confused with right-clicking with the mouse. Changes the kind of cell spawned by right-clicking. Each type of cell has a different color, and you can use all of them at the same time.
	Random: Fills the field with cells at random. Left-click for conway cells, right-click for the one listed in the "Rightclick" button.
	Step: Advances the game by one step.
	Play/Stop: Runs the game.
	Set Custom: Sets the "custom" cell type to behave according to a rule written in the "Golly" format 
		http://en.wikipedia.org/wiki/Life-like_cellular_automaton#Notation_for_rules

CUSTOM CELLS AND THE GOLLY FORMAT

	The "Custom" cell type is unique in that the user can assign it almost any rule by pressing the "set custom" button after typing in a rule. This will change the rule of all "custom" cells, including the ones that are already on the grid. If the rule is invalid, a blank rule will result, killing all custom cells.
	
	Constructing rules for the Golly format is very simple. The rules come in the following format:
		B[...]/S[...]
	Where [...] represents a set of numbers.
	
	The numbers in "B" mean "Birth", which means an dead cell will become alive if it has that number of neighbors. It does NOT mean that a living cell will stay alive. You can use the numbers 1 though 8 (the "B0" rule has been disabled for complicated reasons").
	
	The numbers in "S" mean "Sustain", which means that an living cell will stay alive if it has that many neighbors.
	
	B and S are not mutually exclusive. If a number is in both, it will become alive and say alive if it has that many neighbors. 
	
	If a number is in neither set, it will die if it has that many neighbors.
	
	As an example, Conway's Game of Life is written as "B3/S23", as dead cells give birth if they have three neighbors, and living cells stay alive only if they have two or three neighbors. You can find more examples on wikipedia.
		
NOTES

	Most of the work for this went into two things: allowing for data-defined rules, and allowing multiple types of cellular automata to interact with each other. '
	
	Allowing for data-defined rules was easy. All I needed to do was have define each rule as a <int,int>Dictionary, where the Key was the number of neighbeors and the Value represented what would happen to a cell if it had that many neighbors. In this case, "1" means that cell's life value would be inverted, "2" meant that it would stay the same, "3" meant that it would always be alive, and not being listed at all meant that the cell would always die.
	
	The tricker part was creating a system to let users define their own rules using the Golly format. First, I built a regular expression to test that the input was leigit. Then, I wrote code to parse out the individual numbers and construct a rule dictionary out of them.

	The details on how different kinds of automata interact with each other are written in the source code, but the basic gist is that the game first checks which rules would allow that cell to "give birth" (whether it's empty or not), and then sees how many of it's neighbors have that rule, with the most commonly occurring rule winning out in the end. If there is a tie between two different rules, rule -1 is assigned, which always kills the cell. If the cell has no neighbors at all, it keeps whatever rule that is currently assigned.
	
	Even so, most life-like automata should run exactly like they would in a single-automata system. The only exceptions are "B0" rules, which I found I could not implement in any reasonable manner. Lucky, B0 rules are not only rare, but they are also beyond the scope of the original assignment.
	
	
FURTHER IDEAS FOR GAMIFICATION
	*	A turn-based game (possibly a roguelike), where the enemies move multiply according to the rules of cellular automata.
		*	This idea is probably my favorite, but it would take a bit more time to hammer out how it world work.
	*	A puzzle game where you have to create a specific pattern using a finite number of cells and steps. 
		*	Note that someone has already made this. http://www.kongregate.com/games/squidsquid/the-irregulargame-of-life
	*	An puzzle game where you have to use one type of cell to kill or hinder another type of cell. An example of this would be trying to stop a mass of "Life without Death" cells using only normal Conway cells.
		*	Note that someone has already made this: http://indiegames.com/2011/02/browser_game_pick_conways_infe.html
	*	An arcade game where you have to guide "alive" cells into enemies that are trying to reach a specific objective.
		*	Note that this is semi-implemented. The "Invader Game" scene shows my attempt to make this, but the collision detection system proved too fickle and too show to work all that well. To fix it, I imagine I would have to completely recode the aliens so that they would know what grid squares they were over instead relying on collision detection to do it for me.