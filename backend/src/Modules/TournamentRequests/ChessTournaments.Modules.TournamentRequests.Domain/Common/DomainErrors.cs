namespace ChessTournaments.Modules.TournamentRequests.Domain.Common;

public static class DomainErrors
{
    public static class TournamentRequest
    {
        public static readonly Error NotFound = new(
            "TournamentRequest.NotFound",
            "Tournament request not found"
        );

        public static readonly Error AlreadyApproved = new(
            "TournamentRequest.AlreadyApproved",
            "Tournament request has already been approved"
        );

        public static readonly Error AlreadyRejected = new(
            "TournamentRequest.AlreadyRejected",
            "Tournament request has already been rejected"
        );

        public static readonly Error AlreadyCancelled = new(
            "TournamentRequest.AlreadyCancelled",
            "Tournament request has already been cancelled"
        );

        public static readonly Error CannotApproveNonPending = new(
            "TournamentRequest.CannotApproveNonPending",
            "Only pending requests can be approved"
        );

        public static readonly Error CannotRejectNonPending = new(
            "TournamentRequest.CannotRejectNonPending",
            "Only pending requests can be rejected"
        );

        public static readonly Error CannotCancelNonPending = new(
            "TournamentRequest.CannotCancelNonPending",
            "Only pending requests can be cancelled"
        );

        public static readonly Error InvalidStartDate = new(
            "TournamentRequest.InvalidStartDate",
            "Start date must be in the future"
        );

        public static readonly Error InvalidPlayerRange = new(
            "TournamentRequest.InvalidPlayerRange",
            "Minimum players must be less than or equal to maximum players"
        );

        public static readonly Error InvalidRounds = new(
            "TournamentRequest.InvalidRounds",
            "Number of rounds must be greater than zero"
        );

        public static readonly Error InvalidTimeControl = new(
            "TournamentRequest.InvalidTimeControl",
            "Time in minutes must be greater than zero"
        );

        public static Error RejectionReasonRequired = new(
            "TournamentRequest.RejectionReasonRequired",
            "Rejection reason is required when rejecting a request"
        );
    }
}
