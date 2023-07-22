using ChessChallenge.API;
using System;

public class OldBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move move;
        try
        {
            (move, _) = CalculateMove(board, timer, 5);
        }
        catch (Exception e)
        {
            return board.GetLegalMoves()[0];
        }
        return move;
    }

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    public (Move, Int64) CalculateMove(Board board, Timer timer, int depth)
    {
        if (depth < 1)
        {
            return (new Move(), EvaluateBoard(board));
        }
        Move[] allMoves = board.GetLegalMoves();
        if (allMoves.Length<=0)
        {
            return (new Move(), 0);
        }

        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        Int64 highestValueMove = Int64.MinValue;

        foreach (Move move in allMoves)
        {
            Int64 boardVal;

            if (MoveIsCheckmate(board, move))
            {
                return (move, Int64.MaxValue);
            }

            board.MakeMove(move);
            (_, boardVal) = CalculateMove(board, timer, depth - 1);
            boardVal *= -1;
            board.UndoMove(move);

            if (boardVal > highestValueMove)
            {
                moveToPlay = move;
                highestValueMove = boardVal;
            }
        }

        return (moveToPlay, highestValueMove);
    }

    public Int64 EvaluateBoard(Board board)
    {
        Int64 boardVal = 0;
        foreach (var currentPieceList in board.GetAllPieceLists())
        {
            int isWhite = currentPieceList.IsWhitePieceList == board.IsWhiteToMove ? 1 : -1;
            int currPieceCount = currentPieceList.Count;
            PieceType currPieceType = currentPieceList.TypeOfPieceInList;
            int currPieceValue = pieceValues[(int)currPieceType];

            boardVal += isWhite * currPieceValue * currPieceCount;
        }

        return boardVal;
    }
}