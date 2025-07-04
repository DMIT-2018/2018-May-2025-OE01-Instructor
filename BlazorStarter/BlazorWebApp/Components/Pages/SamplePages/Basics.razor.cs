﻿using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class Basics
    {
        #region Fields
        #region Random Number
        private const string MY_NAME = "Tina";
        private int oddEvenValue;
        #endregion

        #region Text Boxes
        private string emailText = string.Empty;
        private string passwordText = string.Empty;
        private DateTime? dateText = DateTime.Today;
        private MudForm textForm = new();
        private bool formValid = true;
        #endregion

        #region Radio Buttons, Checkboxes, & Text Area
        // Holds the array (represented by []) of string values used
        // to represent the various meal choices.
        private string[] meals = ["breakfast", "second breakfast", "lunch", "dinner"];
        // Used to hold the selected meal value for the Bootstrap based Radio Group
        private string mealBootstrap = "breakfast";
        // Used to hold the selected meal value for the Microsoft component based Radio Group
        private string mealMicrosoft = "breakfast";
        // Used to hold the value (results) of the checkbox
        // Note: Remember bool always initializes as false
        private bool acceptanceBox;
        // Used to hold the text area value
        private string messageBody = string.Empty;
        #endregion

        #region Lists & Sliders
        // Used to hold a posible collection of values
        // representing possible rides
        // -------------------------------
        // A Dictionary is a collection that represents
        // a key and a value, you can define the datatype for both.
        // In this example the key is an int and the value is a string
        // -------------------------------
        // Pretend this is a collection from a database 
        // The data to populate this Dictionary 
        // will be created in a separate method
        private Dictionary<int, string> rides = [];
        // Used to hold the selected value from the rides collection
        // Note: To not show the 0 when the page loads, use a nullable int.
        private int? myRide;
        // Used to hold a possible list of string
        // representing various vacation spots
        private List<string> vacationSpots = [];
        private IEnumerable<string> selectedVacationSpots = [];
        // Used to store the user's selected vacation spot.
        private string vacationSpot = string.Empty;
        // Used to hold the rating Value
        private int reviewRating = 5;
        #endregion

        // Used for the resulting feedback to the user
        // Not part of a specific area of the page so left outside any region
        private string feedback = string.Empty;
        #endregion

        #region Properties
        /// <summary>
        /// Returns a bool value (true/false) depending on if the value set in oddEvenValue is even or not
        /// </summary>
        private bool IsEven
        {
            get
            {
                return oddEvenValue % 2 == 0;
            }
        }
        // Can be written as a simplified return
        // Example:
        // private bool IsEven => oddEvenValue % 2 == 0;

        #endregion

        #region Methods
        // This method is automatically called when the component is initialized
        // This method should ALWAYS be the first method in your partial class if used
        // For best organization, put all override methods at the top before other methods
        // Example: When the page is opened
        protected override void OnInitialized()
        {
            //Call the RandomValue method to perform our custom initialization logic
            RandomValue();

            //Call the 'PopulateCollections' method to populate the predefined data
            //for the collections
            PopulateCollections();

            // Calls the base class OnInitialized method (if any)
            // Note: You do not need to include this unless you have
            // specifically created a new BaseComponent
            // The default OnInitialized methods in the default base component
            // are EMPTY
            // For our class this is NOT needed
            base.OnInitialized();
        }

        /// <summary>
        /// Generates a random number between 0 and 25 using the Random class
        /// </summary>
        private void RandomValue()
        {
            // Create an instance of the Random class to generate random numbers.
            Random rnd = new();

            // Generate a random integer between 0 (inclusive) and 25 (exclusive)
            // Note: Inclusive means that 0 in included as a possibility while
            // exclusive means that 25 is not a possible value that will be generated
            oddEvenValue = rnd.Next(0, 25);
        }
        /// <summary>
        /// Method is called when the user submits the text input to update the resulting feedback.
        /// </summary>
        private void TextSubmit()
        {
            //force the form to validate
            textForm.Validate();
            //Check if it is valid
            if(formValid)
            {
                //Combine the values of emailText, passwordText, and dateText into a feedback message
                //When using string interpolation if using a terinary operator, remember to wrap the terinary operator in ( )
                feedback = $"Email: {emailText}; Password: {passwordText}; Date: {(dateText.HasValue ? dateText.Value.ToString("d"):"No Date")}";
            }
        }

        private string ValidateDate(string date)
        {
            // Check if the date is in the past
            if (dateText.HasValue ? dateText.Value < DateTime.Now: false)
            {
                return "The date cannot be in the past.";
            }
            return null;
        }
        /// <summary>
        /// Handle the selection of the meal from the radio button control
        /// </summary>
        private void HandleMealSelection(ChangeEventArgs e)
        {
            mealBootstrap = e.Value?.ToString() ?? string.Empty;
        }
        /// <summary>
        /// Method is called when the user submits the radio, checkbox,
        /// and text area to update the resulting feedback.
        /// </summary>
        private void RadioCheckAreaSubmit()
        {
            feedback = $"Bootstrap Meal: {mealBootstrap}; Microsoft Meal: {mealMicrosoft}; Acceptance: {acceptanceBox}; Message: {messageBody}";
        }
        /// <summary>
        /// Populates the 'rides' and 'vacationSpots' collections with predefined data.
        /// </summary>
        private void PopulateCollections()
        {
            int i = 1;
            
            // Populates the rides collection with values
            rides.Add(i++, "Car");
            rides.Add(i++, "Bus");
            rides.Add(i++, "Bike");
            rides.Add(i++, "Motorcycle");
            rides.Add(i++, "Boat");
            rides.Add(i++, "Plane");

            // Sort the 'ride' alphabetically based on the Value.
            rides.OrderBy(x => x.Value).ToDictionary();

            // Populates the vacationSpots List
            vacationSpots.Add("California");
            vacationSpots.Add("Caribbean");
            vacationSpots.Add("Cruising");
            vacationSpots.Add("Europe");
            vacationSpots.Add("Florida");
            vacationSpots.Add("Mexico");
        }
        /// <summary>
        /// Method is called when the user submits the lists and slider inputs to update the resulting feedback.
        /// </summary>
        private void ListSliderSubmit()
        {
            //Generate the feedback string incorporating the selected values
            feedback = $"Ride: {(myRide.HasValue ? rides[myRide.Value]:"No Ride Selected")}; Vacation Spot: {vacationSpot}; Review Rating: {reviewRating}";
        }
        #endregion
    }
}
