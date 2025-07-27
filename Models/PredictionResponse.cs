using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace Websiste_Healthcare_Booking_VoVBacSi_main.Models;


public class PredictionResponse
{
    public List<DiseasePrediction> Predictions { get; set; } = new List<DiseasePrediction>();
}