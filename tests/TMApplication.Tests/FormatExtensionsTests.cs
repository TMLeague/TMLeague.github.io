using FluentAssertions;
using TMApplication.Extensions;
using TMModels;

namespace TMApplication.Tests;

public class FormatExtensionsTests
{
    [Fact]
    public void FillParametersTest()
    {
        var divisionForm = new DivisionForm
        {
            League = "sl",
            Season = "8",
            Division = "1",
            Password = "Cersei",
            Contact = "me",
            JudgeName = "Tester",
            MessageSubject = "League, S{season}, D{division}",
            MessageBody = @"Greetings!

Here are your houses for the Silent League, Season {season}, Division {division} in the format house -> game (password):

Stark: {stark} ({starkPassword})
Greyjoy: {greyjoy} ({greyjoyPassword})
Lannister: {lannister} ({lannisterPassword})
Tyrell: {tyrell} ({tyrellPassword})
Martell: {martell} ({martellPassword})
Baratheon: {baratheon} ({baratheonPassword})

Please have a final look at the rules before starting (https://www.thronemaster.net/?goto=community&sub=forum&fid=15&tid=81775#top).
You create only your Stark game with the following settings:
* Default + Pro
* PBEM + Start By Anybody Allowed
* Choose house
* Faceless 
With the title being ‚‚Silent League S{season}/D{division}/G{stark}‘‘ and the password ‚‚{starkPassword}‘‘ (triple check all of this is correct before you create or join a game!).

If there are any problems, please contact {contact} on Discord or Thronemaster and we'll help you with your questions!

Good luck in the tournament and best regards from
{judgeName}, your Manyfaced God this season

Special notice: Please consider the whole board carefully before making any decision that could result in Spoiling or Kingmaking. ‚‚A king who does nothing is no king at all.‘‘ Less poetically, if you allow another player to win or shirk your responsibility off to other player(s) which result in a third players win, not only will you not be able to win the game, but you will also be held accountable for Spoiling/Kingmaking. Making a small sacrifice for the realm should be in your own best interest. So please, be vigilant and take your responsibility seriously.

Special notice 2: THE FACELESS ASPECT IS OF PARAMOUNT IMPORTANCE IN THESE GAMES AS IS THE IDEA OF LETTING ALL OF THEM END AT THE SAME TIME! PLEASE TAKE INTO ACCOUNT THE DELAYING RULE: 

Before the end of each game, the player with the last march must let the Manyfaced God know that the game is ending. He then receives instructions on how to proceed. Not notifying the Manyfaced God results in a -3 penalty. Further penalties may be added for
a) resolving the last march
b) choosing a card (attacker)
c) choosing a card (defender)
d) retreating (loser)
with -1 for each offense. 

Again and to reinforce the previous statement: The faceless and silent aspects of the games are our top priorities."
        };

        var playerHouseGames = new PlayerDraftParameters(
            "Robb",
            1, "abc",
            4, "def",
            3, "ghi",
            7, "jkl",
            9, "mno",
            2, "pqr",
            0, "stu");

        var subject = divisionForm.MessageSubject.FillParameters(divisionForm);
        subject.Should().Be("League, S8, D1");

        var body = divisionForm.MessageBody.FillParameters(divisionForm).FillParameters(playerHouseGames);
        body.Should().Be(@"Greetings!

Here are your houses for the Silent League, Season 8, Division 1 in the format house -> game (password):

Stark: 3 (ghi)
Greyjoy: 9 (mno)
Lannister: 4 (def)
Tyrell: 7 (jkl)
Martell: 2 (pqr)
Baratheon: 1 (abc)

Please have a final look at the rules before starting (https://www.thronemaster.net/?goto=community&sub=forum&fid=15&tid=81775#top).
You create only your Stark game with the following settings:
* Default + Pro
* PBEM + Start By Anybody Allowed
* Choose house
* Faceless 
With the title being ‚‚Silent League S8/D1/G3‘‘ and the password ‚‚ghi‘‘ (triple check all of this is correct before you create or join a game!).

If there are any problems, please contact me on Discord or Thronemaster and we'll help you with your questions!

Good luck in the tournament and best regards from
Tester, your Manyfaced God this season

Special notice: Please consider the whole board carefully before making any decision that could result in Spoiling or Kingmaking. ‚‚A king who does nothing is no king at all.‘‘ Less poetically, if you allow another player to win or shirk your responsibility off to other player(s) which result in a third players win, not only will you not be able to win the game, but you will also be held accountable for Spoiling/Kingmaking. Making a small sacrifice for the realm should be in your own best interest. So please, be vigilant and take your responsibility seriously.

Special notice 2: THE FACELESS ASPECT IS OF PARAMOUNT IMPORTANCE IN THESE GAMES AS IS THE IDEA OF LETTING ALL OF THEM END AT THE SAME TIME! PLEASE TAKE INTO ACCOUNT THE DELAYING RULE: 

Before the end of each game, the player with the last march must let the Manyfaced God know that the game is ending. He then receives instructions on how to proceed. Not notifying the Manyfaced God results in a -3 penalty. Further penalties may be added for
a) resolving the last march
b) choosing a card (attacker)
c) choosing a card (defender)
d) retreating (loser)
with -1 for each offense. 

Again and to reinforce the previous statement: The faceless and silent aspects of the games are our top priorities.");
    }
}