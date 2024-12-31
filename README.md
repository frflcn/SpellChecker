# SpellCheker 5000:
## The WagnerFischer Algorithm
The SpellChecker 5000 utilizes the WagnerFischer algorithm for detecting edit distance between words. The WagnerFischer algorithm counts the least number of operations it takes to get from one word to another. With the operations being: insertions, deletions and substitutions.
- Insertion
  - STING -> ST***R***ING
- Deletion
  - ST***R***ING -> STING
- Substition
  - S***T***RING -> S***P***RING
  
Note that the WagnerFischer algorithm does not consider a swap as an operation:
- Swap
  - S***RT***ING -> S***TR***ING
 
The way the WagnerFischer Algorithm is calculated is by a matrix like so:<br/>
![WagnerFischer Completed](https://github.com/user-attachments/assets/7eb95d0f-c0a9-45a2-847f-722fec425c5c)<br/>
Where every cell correlates to the edit distance between the portions of the words it represents.<br/><br/>

This cell correlates to the distance between "cat" and "carpet"<br/>
![WagnerFischer cat-carpet](https://github.com/user-attachments/assets/ff82cb47-bd55-41c3-912c-d99d31eb81c6)<br/><br/>

And this cell correlates to the distance between "catch" and "carpet"<br/>
![WagnerFischer catch-carpet](https://github.com/user-attachments/assets/21a9d9db-949e-4508-979f-8727c8331be0)<br/><br/>

To Setup the Matrix, write each word with a space in front, down each axis like so:<br/>
![WagnerFischer Empty](https://github.com/user-attachments/assets/ea8c1e88-e07f-4051-b779-3b5390b805ea)<br/><br/>

The first row and column is just 0, 1, 2, 3... as it takes one more insertion to get from the empty string to the next portion of the word.<br/>
![WagnerFischer FirstRow/Column](https://github.com/user-attachments/assets/251957dc-8956-4006-afa2-d5f5fc1569a1)<br/><br/>

Each successive cell is calculated as such: if the letters are the same, the cell is equal to the cell to the upper left diagonal.<br/>
![WagnerFischer LettersAreSame](https://github.com/user-attachments/assets/e4ecce89-a184-4316-8da8-c1447f930797)<br/>
'c' == 'c' therefore, the Yellow cell is assigned the red cell value.<br/><br/>

If the letters are different the cell is equal to the minimum of the three cells up, left and up-left diagonal plus one:<br/>
![WagnerFischer LettersAreDifferent](https://github.com/user-attachments/assets/63f027c0-4de8-4a7a-b60f-fbf75d40feab)<br/>
'c' != 'a' therefore, the Yellow cell is assigned to Min(0, 1, 2) + 1.<br/><br/>

This is carried on until the matrix is complete and the final distance is the cell in the bottom right corner.<br/>
![WagnerFischer Complete](https://github.com/user-attachments/assets/6f796da2-9537-48ca-a220-4babd2dd161a)<br/>
The edit distance between "catch" and "carpet" is 4.<br/><br/>

## Speedups
The WagnerFischer aglorithm is moderately fast on it's own, but when searching a large dictionary and multiple words at a time, it can get bogged down quick.<br/><br/>

To speed up the algorithm I utilized two different tricks:<br/>
1. Reusing the matrix and,
2. Searching only the words with a length that's possible to return a favorable edit distance

### Reusing the Matrix
If you notice, if "carpet" is the word that we are spellchecking and "catch" is from the dictionary; if the previous word from the dictionary was "cat" most of the matrix is already filled out. We can reuse this portion of the matrix to speed up the algorithm and we see approximately a 2.5x increase in speed, with longer dictionaries seeing a bigger increase in speed.

### Searching specific lengths of words
If we are spellchecking a word and want to return some number of suggested corrections. Once we have checked that number of words in the dictionary, we can reduce the search to only the words with length greater than [theLengthOfTheWordWeAreSpellChecking - theHighestScoreInOurSelection] and less then [theLengthOfTheWordWeAreSpellChecking + theHighestScoreInOurSelection]. This is due to the fact that any word outside of this range will require atleast ourHighestScore in number of insertions or deletions to get to our spellchecked word. This method can give on average approximately a 2x speedup in addition to the Reusing the Matrix speedup.<br/><br/>

Using both of these speedups, in total I managed to speed the algorithm up anywhere from 5-10x depending on the dictionary used and the words being checked.<br/><br/>

## The Big Picture
1. The SpellChecker 5000 first loads a newline deliminated dictionary with lowercase words except for proper nouns
   - The (word)dictionary is loaded into a (code)dictionary for quick, yes or no, spellchecks
   - The (word)dictionary is also loaded into a smart list which knows the index of the next word of a specific length. This is used for more involved, word suggestions. And the "smartness" is used for the second speedup (Searching specific lengths of words)
2. The text we are spellchecking is split into lines, then the lines into words
3. Each word is stripped of all symbols on either side such as quotes, parentheses and periods.
4. Each word is checked to see if it is the start of the sentence so that it may be screened for both lowercase words and proper nouns later on
5. Each word is searched against the (word)dictionary(code)dictionary to see if it is misspelled
6. All misspelled words are then put through the WagnerFischer algorithm to return a list of suggested words
7. The Details are then printed out to the user<br/><br/>

## Future Improvements
There are three areas that I see could be improved on in the Spellchecker:
- UI
  - [ ] Allow the user to select the suggested word and have the spellchecker edit the text inplace
  - [ ] Have the context end, at the beginning or end of a word rather than in the middle of a word
- Speed
  - [ ] Start the search with words in the dictionary matching the first letter of the word to spellcheck
  - [ ] Start the search with words with length close to the length of the word being spellchecked, then broaden the search if needed
- Accuracy
  - [ ] Include swaps as well as insertions, deletions and substitutions, as part of the edit distance. Only calculate this more accurate but slower edit distance for suggested words already scored low

## Usage
SpellChecker [[dictionary.txt] file-to-check.txt]<br/><br/>

If you want to use the built-in dictionary you can just pass in a file-to-check.txt. If you want to use your own dictionary, pass it in before the file-to-check.txt. The only requirement for the dictionary is that it is newline deliminated. For best results all words in the dictionary should be lowercase except for proper nouns.

## Download

You can download your operating system specific SpellChecker [here](https://github.com/frflcn/SpellChecker/releases/tag/v1.0)<br/>
If you don't change the filename just be sure to include your operating system specific suffix on the command-line eg.:<br/>
./SpellChecker-linux-arm dictionary.txt text-to-check.txt

