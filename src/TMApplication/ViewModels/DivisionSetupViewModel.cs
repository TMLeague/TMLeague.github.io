namespace TMApplication.ViewModels;

public record DivisionSetupViewModel(
    string Name,
    string InitialMessageSubject, 
    string InitialMessageBody, 
    string NextMainSeason,
    string InitialPassword);