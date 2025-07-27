using System.Text.Json.Serialization;

namespace Websiste_Healthcare_Booking_VoVBacSi_main.Models;


public class DiseasePrediction
{
    public string Disease { get; set; } = "";
    public double Confidence { get; set; }
}