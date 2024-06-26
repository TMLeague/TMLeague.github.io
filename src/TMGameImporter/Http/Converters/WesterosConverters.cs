﻿using TMModels;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http.Converters;

internal interface IWesterosConverter
{
    public IWesterosConverter Parse(LogItem item, WesterosStats events);
}

internal class WesterosPhase1Converter : IWesterosConverter
{
    public IWesterosConverter Parse(LogItem item, WesterosStats events)
    {
        if (item.Message.StartsWith("'Winter is Coming' - New events came up.") &&
            !item.Message.Contains("The crowds of Wildlings have been blown away by the armies of Westeros!"))
        {
            events.AddPhase1(item.Turn, WesterosPhase1.WinterIsComing);
            return new WesterosPhase1Converter();
        }
        if (item.Message.StartsWith("'A Throne of Blades (W)' - The holder of the Iron Throne chose") &&
            !item.Message.Contains("The crowds of Wildlings have been blown away by the armies of Westeros!"))
        {
            events.AddPhase1(item.Turn, WesterosPhase1.ThroneOfBlades);
            return new WesterosPhase2Converter();
        }
        if (item.Message.Contains("got new supplies") &&
            !item.Message.Contains("The crowds of Wildlings have been blown away by the armies of Westeros!"))
        {
            events.AddPhase1(item.Turn, WesterosPhase1.Supply);
            return new WesterosPhase2Converter();
        }
        if (item.Message.Contains("mustered new units:") &&
            !item.Message.Contains("The crowds of Wildlings have been blown away by the armies of Westeros!"))
        {
            events.AddPhase1(item.Turn, WesterosPhase1.Mustering);
            return new WesterosPhase2Converter();
        }
        if (item.Message.Contains("Last Days of Summer (W)") &&
            !item.Message.Contains("The crowds of Wildlings have been blown away by the armies of Westeros!"))
        {
            events.AddPhase1(item.Turn, WesterosPhase1.LastDaysOfSummer);
            return new WesterosPhase2Converter();
        }

        if (item.Message.Contains("Drawn Wildling card:"))
            return new WesterosWildlingsConverter().Parse(item, events);

        return this;
    }
}

internal class WesterosPhase2Converter : IWesterosConverter
{
    public IWesterosConverter Parse(LogItem item, WesterosStats events)
    {
        if (item.Message.StartsWith("'Winter is Coming' - New events came up."))
        {
            events.AddPhase2(item.Turn, WesterosPhase2.WinterIsComing);
            return new WesterosPhase2Converter();
        }
        if (item.Message.StartsWith("'Dark Wings, Dark Words (W)' - The holder of the Messenger Raven chose"))
        {
            events.AddPhase2(item.Turn, WesterosPhase2.DarkWingsDarkWords);
            return new WesterosPhase3Converter();
        }
        if (item.Message.Contains("exerted a power of"))
        {
            events.AddPhase2(item.Turn, WesterosPhase2.ClashOfKings);
            return new WesterosPhase3Converter();
        }
        if (item.Message.Contains("Game of Thrones - Houses consolidated new powers:"))
        {
            events.AddPhase2(item.Turn, WesterosPhase2.GameOfThrones);
            return new WesterosPhase3Converter();
        }
        if (item.Message.Contains("Last Days of Summer (W)"))
        {
            events.AddPhase2(item.Turn, WesterosPhase2.LastDaysOfSummer);
            return new WesterosPhase3Converter();
        }

        return this;
    }
}

internal class WesterosPhase3Converter : IWesterosConverter
{
    public IWesterosConverter Parse(LogItem item, WesterosStats events)
    {
        if (item.Message.StartsWith("'Put To the Sword'"))
        {
            events.AddPhase3(item.Turn, WesterosPhase3.PutToTheSword);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Wildlings Attack: "))
        {
            events.AddPhase3(item.Turn, WesterosPhase3.WildlingAttack);
            if (item.Message.Contains("Wildlings Attack: The Wildlings got cold feet and didn't attack."))
                return new WesterosPhase1Converter();
            return new WesterosWildlingsConverter();
        }
        if (item.Message.Contains("Sea of Storms (W)"))
        {
            events.AddPhase3(item.Turn, WesterosPhase3.SeaOfStorms);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Rains of Autumn (W)"))
        {
            events.AddPhase3(item.Turn, WesterosPhase3.RainsOfAutumn);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Feast for Crows (W)"))
        {
            events.AddPhase3(item.Turn, WesterosPhase3.FeastForCrows);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Web of Lies (W)"))
        {
            events.AddPhase3(item.Turn, WesterosPhase3.WebOfLies);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Storm of Swords (W)"))
        {
            events.AddPhase3(item.Turn, WesterosPhase3.StormOfSwords);
            return new WesterosPhase1Converter();
        }

        return this;
    }
}

internal class WesterosWildlingsConverter : IWesterosConverter
{
    public IWesterosConverter Parse(LogItem item, WesterosStats events)
    {
        if (item.Message.Contains("Drawn Wildling card: Massing on the Milkwater"))
        {
            events.AddWildling(item.Turn, Wildling.MassingOnTheMilkwater);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Drawn Wildling card: A King Beyond the Wall"))
        {
            events.AddWildling(item.Turn, Wildling.AKingBeyondTheWall);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Drawn Wildling card: Mammoth Riders"))
        {
            events.AddWildling(item.Turn, Wildling.MammothRiders);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Drawn Wildling card: Crow Killers"))
        {
            events.AddWildling(item.Turn, Wildling.CrowKillers);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Drawn Wildling card: The Horde Descends"))
        {
            events.AddWildling(item.Turn, Wildling.TheHordeDescends);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Drawn Wildling card: Skinchanger Scout"))
        {
            events.AddWildling(item.Turn, Wildling.SkinchangerScout);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Drawn Wildling card: Rattleshirt's Raiders"))
        {
            events.AddWildling(item.Turn, Wildling.RattleshirtsRaiders);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Drawn Wildling card: Silence at the Wall") ||
            item.Message.Contains("Nothing happens."))
        {
            events.AddWildling(item.Turn, Wildling.SilenceAtTheWall);
            return new WesterosPhase1Converter();
        }
        if (item.Message.Contains("Drawn Wildling card: Preemptive Raid"))
        {
            events.AddWildling(item.Turn, Wildling.PreemptiveRaid);
            return new WesterosPhase1Converter();
        }

        return this;
    }
}
